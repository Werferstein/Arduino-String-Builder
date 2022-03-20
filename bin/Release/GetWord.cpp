#include <Arduino.h>
//The function creates a string of words (#define) separated by one character per word (delimiter). The positions of the words are defined in a list per word.
#define Word_Version           0
#define Word_OK                8
#define Word_to                11
#define Word_off               16
#define Word_on                20
#define Word_auto              23
#define Word_Error             28
#define Word_offline           34
#define Word_Temp              42

const char TextBuffer [] =
"Ver.001/OK/to->/off/on/auto/Error/offline/Temp/";

//The function returns a defined word.
//The words were created by the CPP Arduino String Builder tool.
char* GetWord(uint8_t wordNo, bool print = false, char prefix = ' ', char suffix = 0)
{  
    uint8_t pos = 0;    
    if (prefix != 0) {DisplayBuffer[pos] = prefix; pos = 1;}                //set the prefix char  
    while (TextBuffer[wordNo] !='/')                                        //until the next delimiter char comes
    {        
        DisplayBuffer[pos] = TextBuffer[wordNo];       
        pos ++;
        wordNo++;
    }    
    DisplayBuffer[pos] = 0;
    if (prefix != 0){DisplayBuffer[pos] = suffix; DisplayBuffer[pos+1] = 0;} //set the suffix char
    if (print) Serial.print(DisplayBuffer);
  return DisplayBuffer;
}

