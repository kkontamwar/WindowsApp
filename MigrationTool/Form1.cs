using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Construction;

//using Project = Microsoft.Build.Evaluation.Project;

namespace MigrationTool
{
    public partial class Form1 : Form
    {
        private TreeNode _root = new TreeNode("Solution");
        private TreeNode _filenode = new TreeNode("Files");
        string _path = "";
        private static Solution _solutionFile = null;
        private DirectoryInfo _parentDirectory = null;
        private List<string> ext = new List<string> { ".cs", ".ts", ".config", " *.jpg", "*.gif", "*.cpp", "*.c", "*.htm", "*.html", "*.xsp", "*.asp", "*.xml", "*.h", "*.asmx", "*.asp", "*.atp", "*.bmp", "*.dib", "*.config", "*.sln", "*.txt" };
        static DataTable _projects = null;
        readonly char[] splitchar = new char[] { ' ', ',' };
        readonly string _filepath = Path.GetDirectoryName(Application.ExecutablePath) + "\\Autocomplete.txt";
        AutoCompleteStringCollection _autocompleteList = new AutoCompleteStringCollection();
        private GroupByGrid groupByGrid = null;
        public Form1()
        {
            InitializeComponent();
            groupByGrid = new GroupByGrid();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.LabelEdit = true;
            listView1.FullRowSelect = true;
            listView1.Sorting = SortOrder.Ascending;
            ApplyAutoFilter();
        }

        private void ApplyAutoFilter()
        {
            if (!File.Exists(_filepath))
            {
                File.Create(_filepath);
            }
            else
            {
                using (var reader = new StreamReader(_filepath))
                {
                    while (!reader.EndOfStream)
                        _autocompleteList.Add(reader.ReadLine());
                }

                txtSearchBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                txtSearchBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                txtSearchBox.AutoCompleteCustomSource = _autocompleteList;
            }
        }


        private void GetProjects()
        {
            openFileDialog1.Filter = @"solution files (*.sln)|*.sln";
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.

            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                textBox1.Text = file;
                try
                {
                    _parentDirectory = Directory.GetParent(file);
                    _solutionFile = new Solution(file);
                    treeView1.Nodes.Add(_root);

                    if (_solutionFile.Projects == null) return;
                    var solutionDir = Path.GetDirectoryName(file);

                    var prjFiles = Directory.GetFiles(solutionDir ?? throw new InvalidOperationException(), "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".csproj"));
                    foreach (var prjFile in prjFiles)
                    {
                        //Engine.GlobalEngine.BinPath = @"C:\Windows\Microsoft.NET\Framework\v2.0.xxxxx";

                        // Create a new empty project
                        var project = new Project();

                        // Load a project
                        project.Load(prjFile);

                        if (project.PropertyGroups == null) continue;
                        foreach (BuildPropertyGroup propertyGroup in project.PropertyGroups)
                        {
                            foreach (BuildProperty prop in propertyGroup)
                            {
                                MessageBox.Show(string.Format("{0}:{1}", prop.Name, prop.Value));
                            }
                        }


                        //XmlDocument xmldoc = new XmlDocument();
                        //xmldoc.Load(prjFile);
                        //XmlNamespaceManager ns = new XmlNamespaceManager(xmldoc.NameTable);
                        //ns.AddNamespace("msbld", "http://schemas.microsoft.com/developer/msbuild/2003");
                        //XmlNode node = xmldoc.SelectSingleNode("/Project/PropertyGroup/AssemblyName", ns);

                        //if (node != null)
                        //{
                        //    MessageBox.Show(node.InnerText);
                        //}
                    }


                    foreach (var d in _solutionFile.Projects)
                    {
                        var projNode = new TreeNode(d.ProjectName);
                        _root.Nodes.Add(projNode);
                    }
                }
                catch (IOException ex)
                {
                }
            }
        }

        //Data to be dispaly on the left panel
        //private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    for (int i = 0; i < listView1.Items.Count; i++)
        //    {
        //        if (listView1.Items[i].Selected == true)
        //        {
        //            _path = listView1.Items[i].Name;
        //            textBox1.Text = _path;
        //            listView1.Items.Clear();
        //            //LoadFilesAndDir(_path);
        //        }
        //    }
        //}

        /// <summary>
        /// Browse the solution file to explore its content.
        /// </summary>
        /// <param name="sender">button object</param>
        /// <param name="e">button event argument</param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            GetProjects();
        }

        /// <summary>
        /// Create default datatable header content.
        /// </summary>
        private static void CreateTemplateTable()
        {
            _projects = new DataTable();
            // add column to datatable  
            _projects.Columns.Add("Project Name", typeof(string));
            _projects.Columns.Add("Search Keaywords", typeof(string));
            _projects.Columns.Add("File Name", typeof(string));
            _projects.Columns.Add("Line No.", typeof(int));
            _projects.Columns.Add("Line", typeof(string));
        }

        private static void GetProjectOutPutType()
        {
            //DTE DTE = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE;
            //var project = ((Array) DTE.ActiveSolutionProjects).GetValue(0) as Project;
            //var properties = project.Properties;
            //var ot = properties.Item("OutputType").Value.ToString();s
            //prjOutputType po = (prjOutputType) Enum.Parse(typeof(prjOutputType), ot);
        }

        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            try
            {
                listView1.Items.Clear();
                var selectednode = e.Node;
                treeView1.SelectedNode.ImageIndex = e.Node.ImageIndex;
                selectednode.Expand();
                //txtSearchBox.Text = selectednode.FullPath;

                if (selectednode.Nodes.Count > 0)
                {
                    foreach (TreeNode n in selectednode.Nodes)
                    {

                        var lst = new ListViewItem(n.Text, n.ImageIndex);
                        lst.Name = n.FullPath.Substring(13);
                        listView1.Items.Add(lst);
                    }
                }
                else
                {
                    listView1.Items.Add(selectednode.FullPath, selectednode.Text, selectednode.ImageIndex);
                }
            }
            catch (Exception e1)
            {
            }

        }

        private static void ShowMethods(Type type)
        {
            foreach (var method in type.GetMethods())
            {
                var parameters = method.GetParameters();
                var parameterDescriptions = string.Join
                (", ", method.GetParameters()
                    .Select(x => x.ParameterType + " " + x.Name)
                    .ToArray());

                Console.WriteLine("{0} {1} ({2})",
                    method.ReturnType,
                    method.Name,
                    parameterDescriptions);
            }
        }

        private static void SearchText(string projectName, string filePath, string[] searchTextArray)
        {
            var counter = 0;
            string line;

            // Read the file and display it line by line.
            var file = new StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                //if (line.Contains(textToBeSearch))
                foreach (var textToBeSearch in searchTextArray)
                {
                    if (Regex.IsMatch(line, @"\b" + textToBeSearch + @"\b"))
                        _projects.Rows.Add(projectName, textToBeSearch, Path.GetFileName(filePath), counter, line.Trim());

                }

                counter++;
            }
            file.Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            EnableDisableButtons(false);
            // string searchText = txtSearchBox.Text;

            if (string.IsNullOrEmpty(txtSearchBox.Text))
            {
                MessageBox.Show(@"Please enter text that need to search !");
            }
            else
            {
                var searchTextArray = txtSearchBox.Text.Split(splitchar);
                TextWriter txt = new StreamWriter(_filepath);
                txt.Write(searchTextArray.ToList());
                txt.Close();
                // File.WriteAllLines(_filepath, searchTextArray.ToList());
                ApplyAutoFilter();

                if (_solutionFile.Projects.Count > 0)
                {
                    CreateTemplateTable();

                    foreach (var projDetails in _solutionFile.Projects)
                    {
                        var projDirectory = Path.Combine(_parentDirectory.FullName, projDetails.RelativePath);
                        var directoryInfo = Directory.GetParent(projDirectory);

                        getFilesAndDir(projDetails.ProjectName, directoryInfo, searchTextArray);
                    }

                    if (_projects == null || _projects.Rows.Count <= 0) return;

                    var dsDataset = new DataSet();
                    dsDataset.Tables.Add(_projects);
                    //dataGridView1.DataSource = dsDataset.Tables[0];
                    groupByGrid1.DataSource = dsDataset.Tables[0];
                }
                else
                    MessageBox.Show(@"There are no projects in the solution !");
            }
            EnableDisableButtons(true);
        }

        private void EnableDisableButtons(bool isEnable)
        {
            btnSearch.Enabled = isEnable;
            btnBrowse.Enabled = isEnable;
            //btnBack.Enabled = isEnable;
            //btnForward.Enabled = isEnable;
        }


        private void getFilesAndDir(string projectName, DirectoryInfo dirname, string[] searchTextArray)
        {
            try
            {
                var filesInProject = Directory.GetFiles(dirname.FullName, "*.*", SearchOption.AllDirectories)
                    .Where(s => ext.Contains(Path.GetExtension(s)));

                foreach (var file in filesInProject)
                {
                    SearchText(projectName, file, searchTextArray);
                }



                //foreach (var fi in dirname.GetFiles())
                //{
                //    if (ext.Contains(Path.GetExtension(fi.FullName)))
                //        SearchText(projectName, fi.FullName, searchText);
                //}
                //try
                //{
                //    foreach (var di in dirname.GetDirectories())
                //    {
                //        getFilesAndDir(projectName, di, searchText);
                //    }
                //}
                //catch (Exception ex)
                //{
                //}
            }
            catch (Exception e1)
            {
            }
        }


    }
}
