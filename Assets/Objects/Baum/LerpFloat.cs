using System;

public class LerpFloat {
    float previousValue;
    float currentValue;
    float nextValue;
    float waitTime;
    float passedTime;



    
    public LerpFloat(float value) {
        setValue(nextValue);
        waitTime = 0;
        passedTime = 0;
    }

    public void Aktualisieren(float deltaTime) {
        if(waitTime == 0) {
            currentValue = nextValue;
        }else{
            passedTime = Math.Min(passedTime+deltaTime, waitTime);
            currentValue = (1-passedTime/waitTime) * previousValue + (passedTime/waitTime) * nextValue;
        }
        
    }

    public void setValue(float value) {
        this.previousValue = value;
        this.currentValue = value;
        this.nextValue = value;
        this.waitTime = 0;
    }

    public void setValue(float nextValue, float waitTimeInSeconds) {
        if(this.nextValue != nextValue) {
            this.previousValue = this.currentValue;
            this.nextValue = nextValue;
            passedTime = 0;
            waitTime = waitTimeInSeconds;

        }
    }

    public float getValue() {
        return currentValue;
    }
}