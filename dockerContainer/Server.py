# this is just a very simple server to serve the files in the current directory with CORS enabled

import os
import click

from http.server import HTTPServer, SimpleHTTPRequestHandler, test

class CORSRequestHandler (SimpleHTTPRequestHandler):
    def end_headers (self):
        self.send_header('Access-Control-Allow-Origin', '*')
        SimpleHTTPRequestHandler.end_headers(self)


@click.command()
@click.option('-p', '--port', default=8000, help='Port to listen on')
@click.option('-d', '--dir', default='', help='Directory to serve')
def main(port=8000,dir=""):
    if dir: 
        os.chdir(dir)
    server = HTTPServer(('', port), CORSRequestHandler)
    server.serve_forever()


if __name__ == '__main__':
    main()