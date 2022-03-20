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
using FastColoredTextBoxNS;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CPP_Arduino_String_Builder
{
    public partial class MainForm : Form
    {
        const string CONFIGNAME = "ProgConfig.ini";
        
        
        protected static readonly Platform platformType = PlatformType.GetOperationSystemPlatform();
        public readonly Style BlueBoldStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        public readonly Style BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        public readonly Style BoldStyle = new TextStyle(null, null, FontStyle.Bold | FontStyle.Underline);
        public readonly Style BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Italic);
        public readonly Style GrayStyle = new TextStyle(Brushes.Gray, null, FontStyle.Regular);
        public readonly Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        public readonly Style MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
        public readonly Style MaroonStyle = new TextStyle(Brushes.Maroon, null, FontStyle.Regular);
        public readonly Style RedStyle = new TextStyle(Brushes.Red, null, FontStyle.Regular);
        public readonly Style BlackStyle = new TextStyle(Brushes.Black, null, FontStyle.Regular);

        public Project MyProject = new Project();
        public Config MyConfig = new Config();

        int BlockGUI = 0;
        bool DefineFunction = false;

        // This is the BindingSource that provides data for
        // the datatable
        BindingSource customersBindingSource = new BindingSource();

        public MainForm()
        {
            this.Load += Main_Load;
            
            
            InitializeComponent();
            
           
            fctb.TextChanged += Fctb_TextChanged;

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + CONFIGNAME))
            {
                Config config = AppSettings<Config>.Load(AppDomain.CurrentDomain.BaseDirectory + CONFIGNAME);
                if (config != null)
                {
                    MyConfig = config;
                    if (!string.IsNullOrEmpty(MyConfig.ProjectPath) && File.Exists(MyConfig.ProjectPath))
                    {
                        Project projectTmp = AppSettings<Project>.Load(MyConfig.ProjectPath);
                        if (projectTmp != null)
                        {
                            MyProject = projectTmp;
                        }
                    }
                }
            }        
        }

        private void BuildName(string name)
        {
            this.Text = "CPP Arduino String Builder    " +name;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            LoadDataToForm();
        }

        public void LoadDataToForm() 
        {
            BlockGUI++;
            textBoxProject.Text = MyProject.ProjectName;
            textBoxDelimiter.Text = MyProject.Delimiter;

            if (DefineFunction)
            {
                fctb.Text = MyProject.FunctionCode;
                fctb.BackColor = Color.WhiteSmoke;
                buttonDefinethefunction.Text = "View build";
            }                
            else
            {
                fctb.Text = MyProject.CodeText;
                fctb.BackColor = textBoxProject.BackColor;
                buttonDefinethefunction.Text = "Define the function";
            }

             
                 
            richTextBoxInfo.Text = MyProject.Info;
            textBoxSaveToPath.Text = MyProject.SaveToPath;



            this.customersBindingSource = new BindingSource();
            this.customersBindingSource.DataSource = MyProject.WordList;
            bindingNavigator1.BindingSource = this.customersBindingSource;
            dataGridView1.DataSource = MyProject.WordList;
            dataGridView1.MultiSelect = false;

            //First the form then the split container, otherwise it doesn't work!
            this.Width = MyConfig.Width;
            this.Height = MyConfig.Height;
            splitContainerVertikal.SplitterDistance = MyConfig.splitContainerVerticalPos;
            splitContainerHorizontal.SplitterDistance = MyConfig.splitContainerHorizontalPos;

            if (dataGridView1.Rows.Count > 0) dataGridView1.Rows[0].Selected = true;
            dataGridView1.ColumnHeadersVisible = true;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            BuildName(MyConfig.ProjectPath);
            BlockGUI--;
        }

        private void Fctb_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            SetCustomStyle(e);

            if (BlockGUI > 0) return;
            
            if(DefineFunction)
                MyProject.FunctionCode= fctb.Text;
            else
                MyProject.CodeText = fctb.Text;
                                       
        }


        private void SetCustomStyle(TextChangedEventArgs e)
        {
            //string
            e.ChangedRange.SetStyle(BrownStyle, @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");
            //comments
            e.ChangedRange.SetStyle(GreenStyle, @"//.*$", RegexOptions.Multiline); e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
            //number
            e.ChangedRange.SetStyle(MagentaStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b"); 
            //class name
            e.ChangedRange.SetStyle(BoldStyle, @"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b"); 
            //keyword
            e.ChangedRange.SetStyle(BlueStyle, @"\b(LRESULT|CString|String|WORD|DWORD|TCHAR|BYTE|BOOL|unsigned|signed|int|bool|char|short|long|float|double|string|wchar|wchar_t|__int8|__int16|__int32|__int64|struct|class|enum|interface|if|else|switch|case|break|defalut|return|true|false|for|do|while|continue|goto|new|list|map|using|namespace|private|protected|public|const|delete|cout|cin)\b");
            //Preprocessing command
            e.ChangedRange.SetStyle(MagentaStyle, @"#\b(include|pragma|if|else|elif|ifndef|ifdef|endif|undef|define|line|error)\b|__[^>]*__", RegexOptions.Singleline); 
            //<>
            e.ChangedRange.SetStyle(BrownStyle, @"<[^>]*>", RegexOptions.Singleline); 
            //Pointer
            e.ChangedRange.SetStyle(MagentaStyle, @"\*", RegexOptions.Singleline); 
            //function //
            e.ChangedRange.ClearFoldingMarkers();
            e.ChangedRange.SetFoldingMarkers("{", "}");
            //allow to collapse brackets block
            e.ChangedRange.SetFoldingMarkers(@"/\*", @"\*/");//allow to collapse comment block
        }

        private void textBoxProject_TextChanged(object sender, EventArgs e)
        {
            if (BlockGUI > 0) return;
            MyProject.ProjectName = textBoxProject.Text;
        }

        private void textBoxDelimiter_TextChanged(object sender, EventArgs e)
        {
            if (BlockGUI > 0) return;
            MyProject.Delimiter = textBoxDelimiter.Text;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MyConfig.ProjectPath))
            {
                MyConfig.ProjectPath = AppDomain.CurrentDomain.BaseDirectory;
            }


            FileDialog dialog = new OpenFileDialog();

            dialog.CheckFileExists = true;
            dialog.Title = "Load project from......";
            dialog.CheckPathExists = true;
            dialog.DefaultExt = "*.ADUWORD|*.aduword";
            dialog.AutoUpgradeEnabled = true;
            dialog.FileName = Path.GetFileName(MyConfig.ProjectPath);
            dialog.InitialDirectory = Path.GetDirectoryName(MyConfig.ProjectPath); 

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                MyConfig.ProjectPath = dialog.FileName;

                Project projectTmp =  AppSettings<Project>.Load(MyConfig.ProjectPath);
                if (projectTmp != null)
                {
                    MyProject = projectTmp;
                    LoadDataToForm();                    
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MyConfig.ProjectPath))
                {
                    MyConfig.ProjectPath = AppDomain.CurrentDomain.BaseDirectory;
                }
                
                
                SaveFileDialog dialog = new SaveFileDialog
                {
                    FileName = Path.GetFileName(MyConfig.ProjectPath),
                    InitialDirectory = Path.GetDirectoryName(MyConfig.ProjectPath),
                    //RestoreDirectory = true,                    
                    Filter = "*.ADUWORD|*.aduword",
                    //FilterIndex = 2,
                    CheckPathExists = true
                };

                dialog.CheckFileExists = false;
                dialog.Title = "Save project to......";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    MyConfig.ProjectPath = dialog.FileName;
                    
                    MyProject.Save(MyConfig.ProjectPath);
                 
                    SaveConfig();
                }
                dialog = null;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!"); }
        }

        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0) dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0];
        }

        private void bindingNavigatorMoveLastItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0) dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.Rows.Count-2].Cells[0];
        }

        private void bindingNavigatorMoveNextItem_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridView1.SelectedCells[0].OwningRow.Index;
            if (rowIndex < dataGridView1.Rows.Count-1)
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[rowIndex + 1].Cells[0];
            }
        }

        private void bindingNavigatorMovePreviousItem_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridView1.SelectedCells[0].OwningRow.Index;
            if (rowIndex > 0 )
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[rowIndex-1].Cells[0];
            }
        }

        private void bindingNavigatorMoveFirstItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0) dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
        }




    private void richTextBoxInfo_TextChanged(object sender, EventArgs e)
        {
            if (BlockGUI > 0) return;
            MyProject.Info= richTextBoxInfo.Text;
        }

        /// <summary>
        /// Get all key eventing from form
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (BlockGUI > 0) return base.ProcessCmdKey(ref msg, keyData); else BlockGUI = 0;


            //Build code F5
            if (keyData == Keys.F5)
            {

                return base.ProcessCmdKey(ref msg, keyData);
            }

            //Save all to xml and file---------------------------------------------------------
            if (keyData == (Keys.Control | Keys.S))
            {
                if (File.Exists(MyConfig.ProjectPath))
                {
                    MyProject.Save(MyConfig.ProjectPath);
                    SaveConfig();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void splitContainerHorizontal_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (BlockGUI > 0) return;
            MyConfig.splitContainerHorizontalPos = e.SplitX;           
        }

        private void splitContainerVertikal_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (BlockGUI > 0) return;
            MyConfig.splitContainerVerticalPos = e.SplitY;
           
        }

        private void Main_SizeChanged(object sender, EventArgs e)
        {
            if (BlockGUI > 0) return;
            MyConfig.Width = this.Width;
            MyConfig.Height = this.Height;
            
        }
        private void SaveConfig() 
        {
            MyConfig.Save(AppDomain.CurrentDomain.BaseDirectory + CONFIGNAME);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveConfig();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (BlockGUI > 0) return;
            MyProject.SaveToPath = textBoxSaveToPath.Text;
        }

        private void TB_Path_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MyProject.SaveToPath))
                {
                   MyProject.SaveToPath = AppDomain.CurrentDomain.BaseDirectory;
                }


                SaveFileDialog dialog = new SaveFileDialog
                {
                    FileName = Path.GetFileName(MyProject.SaveToPath),
                    InitialDirectory = Path.GetDirectoryName(MyProject.SaveToPath),
                    //RestoreDirectory = true,                    
                    //Filter = "*.ADUWORD|*.aduword",
                    //FilterIndex = 2,
                    CheckPathExists = false
                };

                dialog.CheckFileExists = false;
                dialog.Title = "Save project to......";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    MyProject.SaveToPath = dialog.FileName;
                    BlockGUI++;
                    textBoxSaveToPath.Text = dialog.FileName;
                    BlockGUI--;
                }
                dialog = null;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ERROR!"); }
        }

        private void buttonBuild_Click(object sender, EventArgs e)
        {
            toolStripStatusLabelOk.Text = String.Empty;
            if (MyProject == null || MyProject.WordList == null || MyProject.WordList.Rows.Count == 0 || string.IsNullOrEmpty(MyProject.Delimiter)) return;

            DefineFunction = false;
            LoadDataToForm();
            
            int pos = 0;
            string DefineBlock = string.Empty;
            string VarBlock = string.Empty;
            int DelimiterLen = MyProject.Delimiter.Trim().Length;
            MyProject.CodeText = String.Empty;
            int step = 1;
            foreach (DataRow item in MyProject.WordList.Rows)
            {
                if (item["Word"].Equals(DBNull.Value) || item.Equals(DBNull.Value)) continue;
                const string DEFINE = "#define ";
                int DEFINE_Length = DEFINE.Length;
                item[0] = step;

                string varP = item.Field<string>("Word_Pos").Trim();
               

                string var = item.Field<string>("Word").Trim();
                int len = var.Length;

                string fill = " ";
                if ((DEFINE_Length + len) < 30)
                {                 
                    for (int i = 0; i < 30 - (DEFINE_Length + len); i++)
                    {
                        fill += " ";
                    }
                }
                                
                DefineBlock += "#define " + var + fill + pos.ToString() + Environment.NewLine;
                VarBlock += varP + MyProject.Delimiter.Trim();
                pos += (varP.Length + DelimiterLen);
                step++;
            }

            if (!string.IsNullOrEmpty(DefineBlock) && !string.IsNullOrEmpty(VarBlock))
            {
                
                MyProject.CodeText =
                    "#include <Arduino.h>" + Environment.NewLine +
                    "//" + MyProject.ProjectName + Environment.NewLine + "/*" + MyProject.Info + "*/" + Environment.NewLine +
                    DefineBlock + Environment.NewLine +
                    "const char TextBuffer [] =" + Environment.NewLine + "\"" + VarBlock + "\";" + Environment.NewLine + Environment.NewLine +
                    MyProject.FunctionCode + Environment.NewLine;

                BlockGUI++;
                fctb.Text = MyProject.CodeText;
                BlockGUI--;

                var exists = File.Exists(MyProject.SaveToPath);
                var fileMode = exists
                    ? FileMode.Truncate    // overwrites all of the content of an existing file
                    : FileMode.CreateNew;  // creates a new file

                string add = DateTime.Now.ToString().Replace(".","").Replace(":", "").Replace(" ", "");

                try
                {
                    if (File.Exists(MyProject.SaveToPath))
                    {
                        File.Move(MyProject.SaveToPath, AppDomain.CurrentDomain.BaseDirectory + Path.GetFileName( MyProject.SaveToPath) + add);
                    }
                    
                    File.WriteAllText(MyProject.SaveToPath, MyProject.CodeText);

                    toolStripStatusLabelOk.Text = "OK";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "ERROR");
                    toolStripStatusLabelOk.Text = "ERROR";
                }
                
            }
        }



        int rowIndex = 0;
        private void buttonUp_Click_1(object sender, EventArgs e)
        {
            // get selected row index
            rowIndex = dataGridView1.SelectedCells[0].OwningRow.Index;

            // create a new row
            DataRow row = MyProject.WordList.NewRow();

            // add values to the new row
            row[0] = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();
            row[1] = dataGridView1.Rows[rowIndex].Cells[1].Value.ToString();
            row[2] = dataGridView1.Rows[rowIndex].Cells[2].Value.ToString();
            //row[3] = int.Parse(dataGridView1.Rows[rowIndex].Cells[3].Value.ToString());

            if (rowIndex > 0)
            {
                // delete the selected row
                MyProject.WordList.Rows.RemoveAt(rowIndex);
                // add the new row 
                MyProject.WordList.Rows.InsertAt(row, rowIndex - 1);
                dataGridView1.ClearSelection();
                // select the new row
                dataGridView1.Rows[rowIndex - 1].Selected = true;
            }
        }

        private void buttonDown_Click_1(object sender, EventArgs e)
        {            
            rowIndex = dataGridView1.SelectedCells[0].OwningRow.Index;
            DataRow row = MyProject.WordList.NewRow();

            row[0] = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();
            row[1] = dataGridView1.Rows[rowIndex].Cells[1].Value.ToString();
            row[2] = dataGridView1.Rows[rowIndex].Cells[2].Value.ToString();
            //row[3] = int.Parse(dataGridView1.Rows[rowIndex].Cells[3].Value.ToString());

            if (rowIndex < dataGridView1.Rows.Count - 2)
            {
                MyProject.WordList.Rows.RemoveAt(rowIndex);
                MyProject.WordList.Rows.InsertAt(row, rowIndex + 1);
                dataGridView1.ClearSelection();
                dataGridView1.Rows[rowIndex + 1].Selected = true;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {            
            customersBindingSource.Position = dataGridView1.SelectedCells[0].OwningRow.Index ;
        }

        private void buttonDefinethefunction_Click(object sender, EventArgs e)
        {
            if (fctb.BackColor == textBoxProject.BackColor)
            {
                DefineFunction = true;
                buttonDefinethefunction.Text = "View build";
            }
            else
            {
                DefineFunction = false;
                buttonDefinethefunction.Text = "Define the function";
            }
            LoadDataToForm();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("www.werferstein.org\r\n\r\nZum Werferstein 36\r\nDE-51570 Windeck-Werfen\r\n\r\n\r\ninfo@werferstein.org", "Ingolf Hill", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

}

