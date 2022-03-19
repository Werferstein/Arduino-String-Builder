/*
    Copyright (c) 2022 
    Ingolf Hill, Zum Werferstein 36, DE-51570 Windeck-Werfen, i.hill@werferstein.org
    
    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Data;

namespace CPP_Arduino_String_Builder
{
    public class Project:AppSettings<Project>
    {
        
        public Project()            
        {
            WordList = new DataTable();
            WordList.TableName = "WordList";
            WordList.Columns.Add("No", typeof(string));
            WordList.Columns.Add("Word", typeof(string));
            WordList.Columns.Add("Word_Pos", typeof(string));
            DataRow row = WordList.NewRow();
            row["No"] = 1;
            row["Word"] = "new word";
            row["Word_Pos"] = 0;
            WordList.Rows.Add(row);

            Delimiter = "/";
            ProjectName = "New project";
            CodeText = string.Empty;
            Info = string.Empty;

            SaveToPath = string.Empty;
            FunctionCode = string.Empty;

            BeginDefine = "//beginDefine";
            EndDefine = "//endDefine";

            BeginInsert = "//beginInsert";
            EndInsert = "//endInsert";
        }

        public string Info { get; set; }
        public string ProjectName { get; set; }
        public DataTable WordList { get; set; }
        public string CodeText { get; set; }
        public string Delimiter { get; set; }
        public string SaveToPath { get; set; }
        public string BeginDefine { get; set; }
        public string EndDefine { get; set; }
        public string BeginInsert { get; set; }
        public string EndInsert { get; set; }
        public string FunctionCode { get; set; }
}


    public class Config : AppSettings<Config>
    {

        public Config()
        {
            ProjectPath = String.Empty;
            splitContainerHorizontalPos = 250;
            splitContainerVerticalPos = 80;
            Width = 900;
            Height = 600;
        }

        public string ProjectPath { get; set; }
        public int splitContainerHorizontalPos { get; set; }
        public int splitContainerVerticalPos { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

    }
}
