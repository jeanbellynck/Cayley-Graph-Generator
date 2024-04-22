#region Imports
# builtin
from enum import Enum
import os
import subprocess
import time
import contextlib

# quasi builtin
import asyncio
from asyncio.subprocess import PIPE, Process

from aiohttp import web

# types
from typing import Callable, List, Union

PathOrString = Union[os.PathLike, str]

#endregion

#region Constants
ROUTE_OBFUSCATION = 'aosijfoaisdoifnCodnifaoGsinf' # todo: remove from git
DEFAULT_PORT = 63910 # todo: remove from git
START_LOG = '"start!"'
HALT_LOG = '"return!"'
PREPARED_LOG = "?help"
STARTUP_COMMAND = os.getenv("GAP_PATH", "/home/johannesh/repos/gap/gap")
# "%appdata%\\GAP\\runtime\\bin\\cygstart %appdata%\\GAP\\runtime\\bin\\bash --login /run-gap.sh" 
# was too complicated to get it to run on windows, in the same process I started it
WORKING_DIRECTORY = None # "%appdata%\\GAP\\runtime\\bin"
ENVIRONMENT_VARIABLES = None # os.environ.copy()

VERBOSE = False # True
WAIT_FOR_COMPLETION_TIMEOUT = 10
WAIT_FOR_COMPLETION_SLEEP_TIME = 0.1
STARTUP_SLEEP_TIME = 0.5
STARTUP_TIMEOUT = 10
PROCESS_TIMEOUT = 60

ACCESS_CONTROL_ORIGIN = "*"

#endregion

#region GAPRunner class

class RunnerStates(Enum):
    WAITING = 1
    RUNNING = 2
    FINISHED = 3

class ErrorStates(Enum):
    NONE = 0
    RETURN_CODE_NONZERO = 1
    NEVER_WAITED = 2
    ABORTED = 3
class GAPRunner:

    def __init__(self, info: str, timeoutFunction: Callable[[float], bool], process: Process) -> None:
        """ Call the async static method newRunner instead """
        self.lines: List[str] = []
        self.process: Process = process
        self._state: RunnerStates = RunnerStates.RUNNING
        self.info = info
        self._creationTime = time.time()
        self._lastLogTime = self._creationTime
        self.timedOut = lambda: timeoutFunction(self._creationTime)

    @staticmethod
    async def newRunner(command: str, workingDirectory: PathOrString | None = None, info: str = "", timeoutFunction: Callable[[float], bool] = lambda x: False):
        self = GAPRunner(info=info, timeoutFunction=timeoutFunction, process = await asyncio.create_subprocess_shell(command, stdin=PIPE, stdout=PIPE, cwd=workingDirectory, env=ENVIRONMENT_VARIABLES))
        print(f"Created new GAP runner with PID {self.process.pid} (process ID as seen in the task manager) and command\n\t{command}.")
        for i in range(round( STARTUP_TIMEOUT/STARTUP_SLEEP_TIME) ):
            logs = await self.updateLog(log=VERBOSE)
            for line in logs:
                if PREPARED_LOG in line:
                    print("The runner is prepared to receive input.")
                    break
            else:
                await asyncio.sleep(STARTUP_SLEEP_TIME)
                continue
            break
        else:
            raise TimeoutError("The runner timed out during creation.")
        self._state = RunnerStates.WAITING
        return self

    async def checkIfProcessTerminated(self):
        with contextlib.suppress(asyncio.TimeoutError):
            await asyncio.wait_for(self.process.wait(), 1e-6)
        return self.process.returncode is not None
    
    async def checkIfWaiting(self):
        for line in await self.updateLog(log=VERBOSE):
            if HALT_LOG in line or "Error" in line:
                print("This runner has switched to the waiting state! PID:", self.process.pid)
                return True
        return False

    async def getState(self) -> RunnerStates:
        if self._state == RunnerStates.FINISHED:
            return self._state

        terminated = await self.checkIfProcessTerminated()
        if not terminated:
            if self._state == RunnerStates.WAITING:
                return self._state
            startedWaiting = await self.checkIfWaiting() # updates the log and _lastLogTime
            if startedWaiting:
                self._state = RunnerStates.WAITING
                return self._state
            if self._lastLogTime < time.time() - PROCESS_TIMEOUT:
                print("-" * 20)
                try:
                    self.process.kill()
                except Exception as e:
                    print("TRIED TO KILL (",e,")", end=' ')
                else:
                    print("KILLED", end = ' ')
                print("RUNNER WITH PID", self.process.pid, "after it didn't output anything for", time.time() -self._lastLogTime, "seconds.")
                print('The last lines of its output are:', *self.lines[-10:] , sep='\n\t')
                print("-" * 20)
                terminated = True
            else:
                self._state = RunnerStates.RUNNING
                return self._state

        if self._state != RunnerStates.FINISHED and terminated:
            print(f"A runner has finished with returncode {self.process.returncode}! PID: {self.process.pid}. {self.info}")
            # if self.process.returncode != 0:
            #     self.errorState == ErrorStates.RETURN_CODE_NONZERO
            self._state = RunnerStates.FINISHED

        return self._state
    
    async def continueRun(self, command) -> bool:
        
        state = await self.getState() 
        while state == RunnerStates.RUNNING:
            await asyncio.sleep(0.4) 
            state = await self.getState() 

        if state == RunnerStates.FINISHED:
            return False

        print("-----------------------------------------")
        print("Continuing a waiting runner. PID:", self.process.pid)
        print("-----------------------------------------")
        self.lines = []
        # await readlines_alreadyWritten(self.process)
        assert self.process.stdin is not None
        self.process.stdin.write(f"{command}\r\n".encode("utf-8"))
        self._state = RunnerStates.RUNNING
        self._lastLogTime = time.time()
        return True

    async def stop(self):
        if not await self.checkIfProcessTerminated():
            try:
                self.process.kill() 
                self.errorState = ErrorStates.ABORTED
            except ProcessLookupError:
                pass
        self._state = RunnerStates.FINISHED

    async def updateLog(self, timeout: float = 0.05, log: bool = False) -> list:
        """ Loads the latest log messages from the process into self.lines and returns them. Don't call this during execution but call self.getState() because else the program might miss that it switched to the waiting state. """
        lines: List[str] = []
        assert self.process.stdout is not None
        while self.process.returncode is None:
            try:
                line = (
                    await asyncio.wait_for(
                        self.process.stdout.readline(),
                        timeout = timeout
                    )
                ).decode("utf-8", errors='replace').strip()
                if log:
                    print('\t\t', line)
                lines.append(line)
            except asyncio.TimeoutError:
                # print("Buffered latex log output exceeded! PID:", process.pid)
                break
            except RuntimeError:
                continue
        else:
            lines.extend((await self.process.stdout.read()).decode('utf-8', errors='replace').splitlines())
        if len(lines) > 0:
            self._lastLogTime = time.time()
            print("\tRead", len(lines), "lines from process", self.process.pid)
            self.lines.extend(lines)
        return lines
#endregion

#region Server

def runServer(port):
    async def handle(request):
        """ the request must come as text of the form 'texFile,outdir' """
        query = (await request.text())
        return web.Response(text=await do_execute(query), headers={"Access-Control-Allow-Origin": ACCESS_CONTROL_ORIGIN})

    async def handleStopServer(request):
        try:
            raise KeyboardInterrupt("actually interrupted by call to stopServer", headers={"Access-Control-Allow-Origin": ACCESS_CONTROL_ORIGIN})
        finally:
            print("Closed by call to /stopServer---")

    async def startup():
        app = web.Application()
        app.add_routes([
            web.post(f'/{ROUTE_OBFUSCATION}', handle),
            web.get(f'/stopServer{ROUTE_OBFUSCATION}', handleStopServer),
            web.options(f'/{ROUTE_OBFUSCATION}', lambda x: web.Response(headers={"Access-Control-Allow-Origin": ACCESS_CONTROL_ORIGIN, "Access-Control-Allow-Methods": "POST", "Access-Control-Allow-Headers": "Content-Type, Access-Control-Allow-Origin"}))
        ])
        return app

    web.run_app(startup(), port=port)

runner = None
async def do_execute(query):
    global runner
    fails = 0
    while fails < 5:
        if not runner or await runner.getState() == RunnerStates.FINISHED:
            runner = await GAPRunner.newRunner(STARTUP_COMMAND, WORKING_DIRECTORY)
            fails += 1
        if await runner.continueRun(START_LOG+";"+query+HALT_LOG+";"):
            break
    else: 
        return "Error: Failed to start a new GAP process after 5 attempts."

    i = 0
    while await runner.getState() == RunnerStates.RUNNING and i < WAIT_FOR_COMPLETION_TIMEOUT/WAIT_FOR_COMPLETION_SLEEP_TIME:
        await asyncio.sleep(WAIT_FOR_COMPLETION_SLEEP_TIME)
        # await runner.updateLog()
        i += 1

    # await runner.updateLog(log = VERBOSE)
    if i >= WAIT_FOR_COMPLETION_TIMEOUT/WAIT_FOR_COMPLETION_SLEEP_TIME:
        runner.lines.insert(0, f"ABORTED AFTER {WAIT_FOR_COMPLETION_TIMEOUT} SECONDS")
        print(f"ABORTED AFTER {WAIT_FOR_COMPLETION_TIMEOUT} SECONDS.")
        if isinstance(runner, GAPRunner):
            print("PID", runner.process.pid)
            await runner.stop()
    st = 0
    en = len(runner.lines)
    for i, line in enumerate(runner.lines):
        if START_LOG in line:
            st = i + 1
        if HALT_LOG in line:
            en = i
            break
    return "\n".join(runner.lines[st:en])
#endregion

if __name__ == "__main__":
    runServer(DEFAULT_PORT)