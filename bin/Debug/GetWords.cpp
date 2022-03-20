#include <Arduino.h>
//Test
#define Word_Version           0
#define Word_to                8
#define Word_off               13
#define Word_on                17
#define Word_auto              20
#define Word_Error             25
#define Word_offline           31
#define Word_Temp              39
#define Word_OK                44

const char TextBuffer [] =
"Ver.001/to->/off/on/auto/Error/offline/Temp/OK/";

char* GetWord(uint8_t wordNo,bool print = false)
{  
    uint8_t pos = 0;    
    while (TextBuffer[wordNo] !='/')
    {        
        DisplayBuffer[pos] = TextBuffer[wordNo];       
        pos ++;
        wordNo++;
    }
    DisplayBuffer[pos] = 0;
    if(print) Serial.print(DisplayBuffer);
  return DisplayBuffer;
}
