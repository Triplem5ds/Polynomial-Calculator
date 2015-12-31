﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CMath.PolynomialEquation;
using CMath.PolynomialSolver;
using CMath.Trie;
using System.Numerics;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting; // For plotting graphs

namespace CalculatorUI
{
    /*
     * Search for TODOs using the `TODO` keyword for the upcoming work.
     */

    /// <summary>
    /// Enum that holds the operation to be stored / processed
    /// </summary>
    /// <!--Whatever opeation to be added later should be added here along with another old-like version of it ex:-->
    /// <!--GCD , OldGCD -->
    public enum OperationTypeEnum
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        Modulus,
        Gcd,
        DerivativeFirst,
        DerivativeSecond,
        SubstitutionFirst,
        SubstitutionSecond,
        SolveFirst,
        SolveSecond,
        DefiniteIntegralFirst,
        DefiniteIntegralSecond,
        
        PolynomialAccepted,

        OldMultiplication,
        OldModulus,
        OldDivision,
        OldAddition,
        OldSubtration,
        OldGcd,
        OldDerivative,
        OldSubstitution,
        OldSolve,
        OldDefiniteIntegral
    }
    public partial class MainForm : Form
    {
        #region Internal Declrations
        internal Polynomial polynomial1, polynomial2, resultPolynomial;
        internal Solver _solverInstance;
        internal List<Complex> roots;
        internal Complex subResult,X;
        internal Thread exitThread, saveThread;
        //If not found wil be created
        internal string filePath = Application.StartupPath + "\\appdata.xml";
        internal string tip = "tip : ";
        //Definite integral
        internal int poly1DefIntegralA = 0, poly1DefIntegralB = 0;
        internal int poly2DefIntegralA = 0, poly2DefIntegralB = 0;
        #endregion

        public MainForm()
        {
            
            InitializeComponent();
            exitThread = new Thread(ExitThread);
            saveThread = new Thread(SaveThread);
            //Initialize display,value members for later history viewing
            historyListBox.DisplayMember = "DisplayName";
            historyListBox.ValueMember = "returnType";
            //Load colerd font properties
            LoadColorFont(polynomial1Text);
            LoadColorFont(polynomial2Text);
            LoadColorFont(resPolyText);
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            LogPanel.AppendText("Log started " + DateTime.Now.ToShortTimeString());
            string[] lines = System.IO.File.ReadAllLines("help.txt");
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Random rand = new Random((int)(DateTime.Now - start).TotalSeconds);
            int n = lines.Length - 1;
            while (n < 0 || n > lines.Length - 1 || String.IsNullOrWhiteSpace(lines[n]))
            {
                n = rand.Next(0, lines.Length - 1);
            }
            tip = "Tip : " + lines[n];
            Polynomial1Chart.Series["Polynomial1"].Color = Properties.Settings.Default.Polynomial1Color;
            Polynomial1Chart.Series["Polynomial2"].Color = Properties.Settings.Default.Polynomial2Color;
            Polynomial1Chart.Series["Result Polynomial"].Color = Properties.Settings.Default.ResultPolyColor;

        }

        #region Multithreading
        /// <summary>
        /// A thread to handle exit process
        /// </summary>
        internal void ExitThread()
        {
          Program._historyTrie.save(filePath);
          Application.Exit();
            
        }
        /// <summary>
        /// A thread to handle save operation
        /// </summary>
        internal void SaveThread()
        {
            Program._historyTrie.save(filePath);
        }
        #endregion
        #region Solve
        /// <summary>
        /// Show roots of polynomial in the roots log panel
        /// </summary>
        public void ShowRoots()
        {
            rootsTextBox.Clear();
            rootsTextBox.Text += "Polynomial roots:\r\n";
            foreach (var item in roots)
            {
                if (item.Imaginary == 0)
                    rootsTextBox.Text += item.Real.ToString() + "\r\n";
                else
                {
                    rootsTextBox.Text += item.Real.ToString() + " ";
                    if (item.Imaginary > 0)
                    {
                        rootsTextBox.Text += "+ " + item.Imaginary.ToString() + "i" + "\r\n";
                    }
                    else
                    {
                        rootsTextBox.Text += "- " + item.Imaginary.ToString().Substring(1);
                        rootsTextBox.AppendText("i\r\n");
                    }
                }
            }
        }
        /// <summary>
        /// Instantiate solver with a polynomial, and returns the reults in complex numbers list.
        /// </summary>
        /// <param name="_poly"></param>
        /// <param name="result"></param>
        void SolvePolynomial(Polynomial _poly, ref List<Complex> result)
        {
            _solverInstance = new Solver(_poly);
            roots = _solverInstance.solve().ToList();
        }
        #endregion
        #region History
        /// <summary>
        /// Logs an operation into the Log Panel and History
        /// </summary>
        /// <param name="_operation">Operation type to be logged</param>
        public void LogOperation(OperationTypeEnum _operation)
        {
            HistoryLog hL;

            switch (_operation)
            {
                case OperationTypeEnum.Addition:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Added Polynomials.");
                    Program._historyTrie.insert(polynomial1, '+', polynomial2, resultPolynomial);
                    hL = new HistoryLog(polynomial1, polynomial2, '+', DateTime.Now.TimeOfDay.ToString(), resultPolynomial);
                    LogItem(hL);
                    break;
                case OperationTypeEnum.Subtraction:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Subtracted Polynomials.");
                    Program._historyTrie.insert(polynomial1, '-', polynomial2, resultPolynomial);
                    hL = new HistoryLog(polynomial1, polynomial2, '-', DateTime.Now.TimeOfDay.ToString(), resultPolynomial);
                    LogItem(hL);
                    break;
                case OperationTypeEnum.Multiplication:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Multiplied Polynomials.");
                    Program._historyTrie.insert(polynomial1, '*', polynomial2, resultPolynomial);
                    hL = new HistoryLog(polynomial1, polynomial2, '*', DateTime.Now.TimeOfDay.ToString(), resultPolynomial);
                    LogItem(hL);
                    break;
                case OperationTypeEnum.Modulus:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Reminder Polynomial Calculated.");
                    Program._historyTrie.insert(polynomial1, '%', polynomial2, resultPolynomial);
                    hL = new HistoryLog(polynomial1, polynomial2, '%', DateTime.Now.TimeOfDay.ToString(), resultPolynomial);
                    LogItem(hL);
                    break;
                case OperationTypeEnum.Division:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Divided Polynomials.");
                    Program._historyTrie.insert(polynomial1, '/', polynomial2, resultPolynomial);
                    hL = new HistoryLog(polynomial1, polynomial2, '/', DateTime.Now.TimeOfDay.ToString(), resultPolynomial);
                    LogItem(hL);
                    break;
				case OperationTypeEnum.Gcd:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Greatest Common Divisor Polynomial computed.");
                    Program._historyTrie.insert(polynomial1, 'g', polynomial2, resultPolynomial);
                    hL = new HistoryLog(polynomial1, polynomial2, 'g', DateTime.Now.TimeOfDay.ToString(), resultPolynomial);
                    LogItem(hL);
                    break;
                case OperationTypeEnum.SolveFirst:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " First Polynomial Solved.");
                    Program._historyTrie.insert(polynomial1, '=', roots);
                    hL = new HistoryLog(polynomial1, roots, DateTime.Now.TimeOfDay.ToString());
                    LogItem(hL);
                    break;
                case OperationTypeEnum.SolveSecond:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Second Polynomial Solved.");
                    Program._historyTrie.insert(polynomial2, '=', roots);
                    hL = new HistoryLog(polynomial2, roots, DateTime.Now.TimeOfDay.ToString());
                    LogItem(hL);
                    break;
                case OperationTypeEnum.DerivativeFirst:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " First Polynomial Diffrentiated.");
                    Program._historyTrie.insert(polynomial1, '^',resultPolynomial);
                    hL = new HistoryLog(polynomial1,new Polynomial(),'^', DateTime.Now.TimeOfDay.ToString(),resultPolynomial);
                    LogItem(hL);
                    break;
                case OperationTypeEnum.DerivativeSecond:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Second Polynomial Diffrentiated.");
                    Program._historyTrie.insert(polynomial2, '^',resultPolynomial);
                    hL = new HistoryLog(new Polynomial(), polynomial2, '^', DateTime.Now.TimeOfDay.ToString(), resultPolynomial);
                    LogItem(hL);
                    break;
                case OperationTypeEnum.SubstitutionFirst:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Substituted in First Polynomial.");
                    Program._historyTrie.insert(polynomial1, X, subResult);
                    hL = new HistoryLog(polynomial1, new Polynomial(), 's', DateTime.Now.TimeOfDay.ToString(), resultPolynomial);
                    LogItem(hL);
                    break;
                case OperationTypeEnum.SubstitutionSecond:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Substituted in Second Polynomial.");
                    Program._historyTrie.insert(polynomial2, X, subResult);
                    hL = new HistoryLog(new Polynomial(), polynomial2, 's', DateTime.Now.TimeOfDay.ToString(), resultPolynomial);
                    LogItem(hL);
                    break;
                    //TODO Implement definite Integral
                case OperationTypeEnum.DefiniteIntegralFirst:
                    break;
                case OperationTypeEnum.DefiniteIntegralSecond:
                    break;
                case OperationTypeEnum.OldDivision:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Division retrieved.");
                    break;
                case OperationTypeEnum.OldModulus:
                    LogPanel.Text += ("\r\n" + DateTime.Now.ToShortTimeString() + " Reminder Polynomials.");
                    break;
                case OperationTypeEnum.OldMultiplication:
                    LogPanel.AppendText("\r\n" + DateTime.Now.ToShortTimeString() + " Multiplication retrieved.");
                    break;
                case OperationTypeEnum.OldSubtration:
                    LogPanel.AppendText("\r\n" + DateTime.Now.ToShortTimeString() + " Subtraction retrieved.");
                    break;
                case OperationTypeEnum.OldAddition:
                    LogPanel.AppendText("\r\n" + DateTime.Now.ToShortTimeString() + " Addition retrieved.");
                    break;
                case OperationTypeEnum.OldDerivative:
                    LogPanel.AppendText("\r\n" + DateTime.Now.ToShortTimeString() + " Derivative retrieved.");
                    break;
                case OperationTypeEnum.OldSubstitution:
                    LogPanel.AppendText("\r\n" + DateTime.Now.ToShortTimeString() + " Substitution retrieved.");
                    break;
                case OperationTypeEnum.OldSolve:
                    LogPanel.AppendText("\r\n" + DateTime.Now.ToShortTimeString() + " Polynomial root retrieved.");
                    break;
                case OperationTypeEnum.PolynomialAccepted:
                    LogPanel.AppendText("\r\n" + DateTime.Now.ToShortTimeString() + " Polynomial entered.");
                    break;
                case OperationTypeEnum.OldDefiniteIntegral:
                    LogPanel.AppendText("\r\n" + DateTime.Now.ToShortTimeString() + " Polynomial definite integral retrieved.");
                    break;
                default:
                    return;
            }
        }
        /// <summary>
        /// Adds item to HistoryBox
        /// </summary>
        /// <param name="_log">History Log to insert</param>
        internal void LogItem(HistoryLog _log)
        {
            switch (_log.Operation)
            {
                case '+':
                    _log.DisplayName += " Addition";
                    break;
                case '-':
                    _log.DisplayName += " Subtraction";
                    break;
                case '*':
                    _log.DisplayName += " Multiplication";
                    break;
                case '/':
                    _log.DisplayName += " Division";
                    break;
                case '%':
                    _log.DisplayName += " Modulus";
                    break;
                case '=':
                    _log.DisplayName += " Root finding";
                    break;
                case 'g':
                    _log.DisplayName += " GCD";
                    break;
                case '^':
                    _log.DisplayName += " Derivative";
                    break;
                case 's':
                    _log.DisplayName += " Substitution";
                    break;
                    //TODO Implement Definite Integral
            }
            historyListBox.Items.Add(_log);
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            HistoryLogViewForm hForm = new HistoryLogViewForm(historyListBox.SelectedItem as HistoryLog);
            showForm(hForm, this, setClose);
        }

        #endregion
       
        /// <summary>
        /// Performs the requested operation from the pressed button
        /// </summary>
        /// <!--Casts the sender as GlassButton then switch over the Tag property then execute operation-->
        private void PerformOperation(object sender, EventArgs e)
        {
            Polynomial searchResult;
            object dynamicResult;
            try
            {
                rootsTextBox.Clear();
                switch ((sender as Glass.GlassButton).Tag as string)
                {
                    case "Add":
                        if (Program._historyTrie.try_search(polynomial1, '+', polynomial2, out searchResult))
                        {
                            PolynomialParse(searchResult, resPolyText, true);
                            LogOperation(OperationTypeEnum.OldAddition);
                        }
                        else
                        {
                            resultPolynomial = polynomial1 + polynomial2;
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.Addition);
                        }
                        break;
                    case "Subtract":
                        if (Program._historyTrie.try_search(polynomial1, '-', polynomial2, out searchResult))
                        {
                            PolynomialParse(searchResult, resPolyText, true);

                            LogOperation(OperationTypeEnum.OldSubtration);
                        }
                        else
                        {
                            resultPolynomial = polynomial1 - polynomial2;
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.Subtraction);
                        }
                        break;
                    case "Multiply":
                        if (Program._historyTrie.try_search(polynomial1, '*', polynomial2, out searchResult))
                        {
                            PolynomialParse(searchResult, resPolyText, true);
                            LogOperation(OperationTypeEnum.OldMultiplication);
                        }
                        else
                        {
                            resultPolynomial = polynomial1 * polynomial2;
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.Multiplication);
                        }
                        break;
                    case "Division":
                        if (Program._historyTrie.try_search(polynomial1, '/', polynomial2, out searchResult))
                        {
                            PolynomialParse(searchResult, resPolyText, true);
                            LogOperation(OperationTypeEnum.OldDivision);
                        }
                        else
                        {
                            resultPolynomial = polynomial1 / polynomial2;
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.Division);
                        }
                        break;
                    case "Modulus":
                        if (Program._historyTrie.try_search(polynomial1, '%', polynomial2, out searchResult))
                        {
                            PolynomialParse(searchResult, resPolyText, true);
                            LogOperation(OperationTypeEnum.OldModulus);
                        }
                        else
                        {
                            resultPolynomial = polynomial1 % polynomial2;
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.Modulus);
                        }
                        break;
                    case "GCD":
                        if (Program._historyTrie.try_search(polynomial1, 'g', polynomial2, out searchResult))
                        {
                            PolynomialParse(searchResult, resPolyText, true);
                            LogOperation(OperationTypeEnum.OldGcd);
                        }
                        else
                        {
                            resultPolynomial = Polynomial.__gcd(polynomial1, polynomial2);
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.Gcd);
                        }
                        break;
                    case "Find X1":
                        if (Program._historyTrie.try_search(polynomial1, '=', out dynamicResult))
                        {
                            roots = (List<Complex>)dynamicResult;
                            LogOperation(OperationTypeEnum.OldSolve);
                        }
                        else
                        {
                            SolvePolynomial(polynomial1, ref roots);
                            LogOperation(OperationTypeEnum.SolveFirst);
                        }
                        ShowRoots();
                        break;
                    case "Find X2":
                        if (Program._historyTrie.try_search(polynomial2, '=', out dynamicResult))
                        {
                            roots = (List<Complex>)dynamicResult;
                            LogOperation(OperationTypeEnum.OldSolve);
                        }
                        else
                        {
                            SolvePolynomial(polynomial2, ref roots);
                            LogOperation(OperationTypeEnum.SolveSecond);

                        }
                        ShowRoots();
                        break;
                    case "derivative1":
                        if (Program._historyTrie.try_search(polynomial1, '^', out dynamicResult))
                        {
                            searchResult = (Polynomial)dynamicResult;
                            PolynomialParse(searchResult, resPolyText, true);
                            LogOperation(OperationTypeEnum.OldDerivative);
                        }
                        else
                        {
                            resultPolynomial = Polynomial.derivative(polynomial1);
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.DerivativeFirst);
                        }
                        break;
                    case "derivative2":
                        if (Program._historyTrie.try_search(polynomial2, '^', out dynamicResult))
                        {
                            searchResult = (Polynomial)dynamicResult;
                            PolynomialParse(searchResult, resPolyText, true);
                            LogOperation(OperationTypeEnum.OldDerivative);
                        }
                        else
                        {
                            resultPolynomial = Polynomial.derivative(polynomial2);
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.DerivativeSecond);
                        }
                        break;
                    case "Sub1":
                        if (!TryParseComplex(textBox1.Text, out X))
                        {
                            MessageBox.Show("Wrong complex format");
                            return;
                        }
                        if (Program._historyTrie.try_search(polynomial1, X, out subResult))
                        {
                            searchResult = new Polynomial();
                            searchResult.Add(new Term(0,subResult));
                            PolynomialParse(searchResult, resPolyText, true);
                            LogOperation(OperationTypeEnum.OldSubstitution);
                        }
                        else
                        {
                            subResult = polynomial1.substitute(X);
                            resultPolynomial = new Polynomial();
                            resultPolynomial.Add(new Term(0, subResult));
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.SubstitutionFirst);
                        }
                        break;
                    case "Sub2":
                        if (!TryParseComplex(textBox2.Text, out X))
                        {
                            MessageBox.Show("Wrong complex format");
                            return;
                        }
                        if (Program._historyTrie.try_search(polynomial2, X, out subResult))
                        {
                            searchResult = new Polynomial();
                            searchResult.Add(new Term(0,subResult));
                            PolynomialParse(searchResult, resPolyText, true);
                            LogOperation(OperationTypeEnum.OldSubstitution);
                        }
                        else
                        {
                            subResult = polynomial2.substitute(X);
                            resultPolynomial = new Polynomial();
                            resultPolynomial.Add(new Term(0, subResult));
                            PolynomialParse(resultPolynomial, resPolyText, true);
                            LogOperation(OperationTypeEnum.SubstitutionSecond);
                        }
                        break;
                        //TODO Implement Definite Integral HERE
                    case "DefInt1":
                        break;
                    case "DefInt2":
                        break;
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
    
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (saveThread.IsAlive)
            {
                polynomial1Text.Enabled = false;
                polynomial2Text.Enabled = false;
                tipLabel.Text = "Please wait saving...";
            }
            else
            {
                polynomial1Text.Enabled = true;
                polynomial2Text.Enabled = true;
                tipLabel.Text = tip;
            }

            toolStripStatusLabel1.Text = DateTime.Now.ToShortDateString() + ' ' + DateTime.Now.ToShortTimeString();
        }

        //TODO Fix number entry : i : returns false.
        /// <summary>
        /// Tries to parse a string into a Complex Number
        /// </summary>
        /// <param name="_number">String format of number</param>
        /// <param name="_ref">Complex number to store parsed,null if failed</param>
        /// <returns>True if succeded</returns>
        internal bool TryParseComplex(string _number,out Complex _ref)
        {
            _number = _number.ToLower();
            _ref = new Complex();
            if (String.IsNullOrWhiteSpace(_number))
            {
                return false;
            }
            _number = _number.Replace(" ","");
            double _real,_img;
            if(!_number.Contains('+'))
            {
                if (!double.TryParse(_number, out _real))
                    return false;
                _ref = new Complex(_real,0.0);
                return true;
            }
            string real = _number.Split(new char[]{'+'},StringSplitOptions.RemoveEmptyEntries)[0];
            if (real == "")
                return false;
            if (!double.TryParse(real, out _real))
                return false;
            string complex = _number.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries)[1];
            if (complex == "")
                return false;
            if (complex[complex.Length - 1] != 'i') 
                return false;
            complex = complex.Substring(0, complex.Length - 1);
            if (!double.TryParse(complex, out _img))
                return false;
            _ref = new Complex(_real, _img);
            return true;
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveThread = new Thread(SaveThread);
            saveThread.Start();
        }
        // Load history logs

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Properties.Settings.Default.AskOnExit)
            {
                if (MessageBox.Show("Are you sure you want to exit?", " Exit ?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    exitThread.Start();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                exitThread.Start();
                
            }
        }

       /// <summary>
       /// Clears items stored in log, disposes history trie and deletes history log file
       /// reinitiates the log panel, polynomials input text boxes,roots log and history log
       /// reinitiate the colored font settings.
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear all past operations ?\n this action cannot be reverse", "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.Yes)
            {
                System.IO.File.Delete(filePath);
                Program._historyTrie.Dispose();
                historyListBox.Items.Clear();
                LogPanel.Text = "Log started " + DateTime.Now.ToShortTimeString();
                polynomial1Text.Text = "First Polynomial";
                polynomial2Text.Text = "Second Polynomial";
                resPolyText.Text = "Result";
                rootsTextBox.Clear();
                LoadColorFont(polynomial1Text);
                LoadColorFont(polynomial2Text);
                LoadColorFont(resPolyText);
            }

        }

       #region Form Switch Handlers
        /// <summary>
        /// Close handler of the form, applies settings on close and renables the owner form
        /// </summary>
       public void setClose(object sender, FormClosedEventArgs e)
        {
            this.Enabled = true;
            //If customization form
            if (((sender as Form).Tag) == "cstForm")
            {
                LoadColorFont(polynomial1Text);
                LoadColorFont(polynomial2Text);
                LoadColorFont(resPolyText);
                Polynomial1Chart.Series["Polynomial1"].Color = Properties.Settings.Default.Polynomial1Color;
                Polynomial1Chart.Series["Polynomial2"].Color = Properties.Settings.Default.Polynomial2Color;
                Polynomial1Chart.Series["Result Polynomial"].Color = Properties.Settings.Default.ResultPolyColor;
            }
        }
       /// <summary>
       /// Shows a form and disable the owner form till user closes the on top form
       /// </summary>
       /// <param name="frm">Form to show</param>
       /// <param name="owner">Form to be disabled</param>
       /// <param name="closeHandle">Handler of enable/disable</param>
       private void showForm(Form frm, Form owner, FormClosedEventHandler closeHandle)
        {
            frm.Owner = owner;
            frm.Show();
            frm.Activate();
            owner.Enabled = false;
            frm.FormClosed += new FormClosedEventHandler(closeHandle);
        }
       private void aboutUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(new Credits(), this, setClose);
        }
       private void customizeToolStripMenuItem_Click(object sender, EventArgs e)
       {
           showForm(new CustomizeForm(), this, setClose);
       }
       private void licenseToolStripMenuItem_Click(object sender, EventArgs e)
       {
           showForm(new LicenseForm(), this, setClose);
       }
       #endregion
       #region RTF Handling

       #region Parsing Algorithms
       /// <summary>
       /// Parse polynomial from rich text box
       /// </summary>
       /// <param name="_rtBox">RichTextBox that contains the data</param>
       /// <returns></returns>
       internal Polynomial PolynomialParse(RichTextBox _rtBox)
       {
           //string _deg = "", _coeff = "";
           SortedList<int, Complex> sList = new SortedList<int, Complex>();

            string expr = _rtBox.Text;
            int delay = 0;

            for (int i = 0; i < _rtBox.TextLength; i++)
            {
                _rtBox.SelectionStart = i;
                _rtBox.SelectionLength = 1;
                if (_rtBox.SelectionCharOffset > 0)
                    expr = expr.Insert(i + (delay++), "^");
            }

            expr = expr.Replace("-X", "-1X");
            expr = expr.Replace("-x", "-1x");
            expr = expr.Replace("+X", "+1X");
            expr = expr.Replace("+x", "+1x");

            if (expr.StartsWith("X",true,System.Globalization.CultureInfo.CurrentCulture))
                expr = "1" + expr;

            string[] terms =expr.Replace("-", "+-").Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in terms)
            {
                if (s.Contains('x') || s.Contains('X'))
                {
                    if (s.Contains('^'))
                    {
                        string[] cofpow = s.Split(new string[] { "x^", "X^" }, StringSplitOptions.RemoveEmptyEntries);
                        sList.Add(int.Parse(cofpow[1]), new Complex(double.Parse(cofpow[0]), 0)); // N Coeff
                    }
                    else
                    {
                        string ts = s.Replace("X", "");
                        ts = ts.Replace("x", "");
                        if (ts == "")
                            ts = "1";
                        sList.Add(1, new Complex(double.Parse(ts), 0)); //Coeff 1 
                    }
                }
                else
                    sList.Add(0, new Complex(double.Parse(s), 0)); //Free term
            }
            return new Polynomial(sList);
       }
       /// <summary>
       /// Parse polynomial to RichTextBox with superscript and base considration
       /// </summary>
       /// <param name="_polynomial">Polynomial to be parsed</param>
       /// <param name="_rtBox">RichTextBox to parse to</param>
       internal void PolynomialParse(Polynomial _polynomial, RichTextBox _rtBox, bool isResult)
        {
            if (isResult)
            {
                _rtBox.Text = "";
            }
            for (int i = _polynomial.Count - 1; i >= 0; i--)
            {
                if (_polynomial[i].Degree == 0)
                {
                    _rtBox.AppendText(_polynomial[i].Coefficient.Imaginary == 0 ? _polynomial[i].Coefficient.Real.ToString() : _polynomial[i].Coefficient.ToString());
                    if (i != 0)
                    {
                        if (_polynomial[i - 1].Coefficient.Real > 0)
                            _rtBox.AppendText("+");
                    }
                }
                else
                {
                    if (_polynomial[i].Coefficient.Real != 1 || _polynomial[i].Coefficient.Imaginary != 0)
                        _rtBox.AppendText(_polynomial[i].Coefficient.Imaginary == 0 ? _polynomial[i].Coefficient.Real.ToString() : _polynomial[i].Coefficient.ToString());
                    _rtBox.AppendText("X");
                    _rtBox.SelectionCharOffset = 7;
                    if (_polynomial[i].Degree > 1)
                        _rtBox.AppendText(_polynomial[i].Degree.ToString());
                    _rtBox.SelectionCharOffset = 0;
                    if (i != 0)
                    {
                        if (_polynomial[i - 1].Coefficient.Real > 0)
                            _rtBox.AppendText("+");
                    }
                }
            }
            if (_polynomial.isnull())
            {
                _rtBox.Text = "0";
            }
            if (isResult)
            {
                LoadColorFont(_rtBox);
                DrawOnGraph(_polynomial, Polynomial1Chart, "Result Polynomial");
            }
        }

       #endregion
        /// <summary>
        /// Loads color and font settings to polylnomials text boxes
        /// </summary>
        /// <param name="_rbox"></param>
       internal void LoadColorFont(RichTextBox _rbox)
       {
           for (int i = 0; i < _rbox.TextLength; i++)
           {
               _rbox.SelectionStart = i;
               _rbox.SelectionLength = 1;
               _rbox.SelectionFont = Properties.Settings.Default.Font;
               _rbox.SelectionColor = Properties.Settings.Default.Color;
           }
       }
       /// <summary>
        /// Enables superscript writing for power
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       private void SuperScriptOnX(object sender, EventArgs e)
       {
           RichTextBox rtbox = sender as RichTextBox;
            if (rtbox.TextLength != 0)
            {
                char lastChar = rtbox.Text.Last();
                if (lastChar == 'X' || lastChar == 'x')
                {
                    rtbox.SelectionCharOffset = 7;
                }
            }
       }
        /// <summary>
        /// Switch to base and parse polynomial on pressing enter
        /// </summary>
        /// <param name="sender">Control sender</param>
        /// <param name="e">KeyPress Param</param>
       private void BaseOnEnter(object sender, KeyPressEventArgs e)
       {
           try
           {
               RichTextBox rtBox = sender as RichTextBox;
               if (e.KeyChar == (char)Keys.Enter)
               {
                   if (rtBox.SelectionCharOffset > 0)
                   {
                       rtBox.SelectionCharOffset = 0;
                   }
                   else
                   {
                       if ((rtBox.Tag as string) == "Poly1")
                       {
                           polynomial1 = PolynomialParse(rtBox);
                           DrawOnGraph(polynomial1, Polynomial1Chart, "Polynomial1");
                       }
                       else if ((rtBox.Tag as string) == "Poly2")
                       {
                           polynomial2 = PolynomialParse(rtBox);
                           DrawOnGraph(polynomial2, Polynomial1Chart, "Polynomial2");
                       }

                       LogOperation(OperationTypeEnum.PolynomialAccepted);

                   }
               }
           }
           catch (Exception ex)
          {
               MessageBox.Show(ex.Message);
           }
       }

       private void DrawOnGraph(Polynomial equation, Chart graph,string name)
       {
           int minValue = Properties.Settings.Default.MinValue;
           int maxValue = Properties.Settings.Default.MaxValue;
           this.SuspendLayout();
           graph.Series[name].Points.Clear();
           for (int i = minValue; i < maxValue; i++)
           {
               graph.Series[name].Points.AddXY
                               (i, equation.substitute(i).Real);
               graph.Series[name].Points.AddXY
                               (i,equation.substitute(i).Real);
           }
           graph.Series[name].ChartType = SeriesChartType.FastLine;
           
           this.ResumeLayout();
           graph.Refresh();
       }

       #endregion
       
        /// <summary>
        /// Handler of placeholder for text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaceHolder(object sender,EventArgs e)
       {
           var txtBox = (sender as TextBox);
           Complex tester;
           if (txtBox.Text != "" && TryParseComplex(txtBox.Text,out tester))
           {
               switch (txtBox.Tag as string)
               {
                   case "Def1A":
                       poly1DefIntegralA = int.Parse(txtBox.Text);
                       break;
                   case "Def1B":
                       poly1DefIntegralB = int.Parse(txtBox.Text);
                       break;
                   case "Def2A":
                       poly2DefIntegralA = int.Parse(txtBox.Text);
                       break;
                   case "Def2B":
                       poly2DefIntegralB = int.Parse(txtBox.Text);
                       break;
               }
          }
           else
           {
               switch (txtBox.Tag as string)
               {
                   case "Def1A":
                       txtBox.Text = "Poly1. a";
                       break;
                   case "Def1B":
                       txtBox.Text = "Poly1. b";
                       break;
                   case "Def2A":
                       txtBox.Text = "Poly2. a";
                       break;
                   case "Def2B":
                       txtBox.Text = "Poly2. b";
                       break;
                   case "Poly.1x":
                       txtBox.Text = "Poly.1 X";
                       break;
                   case "Poly.2x":
                       txtBox.Text = "Poly.2 X";
                       break;
               }
               txtBox.ForeColor = SystemColors.WindowFrame;
           }
       }

        private void PlaceHolderRemove(object sender,EventArgs e)
        {
            var txtBox = sender as TextBox;
            txtBox.ForeColor = Color.Black;
            txtBox.Text = "";
        }
        //TODO Merge function with the original place holder
       
        private void restorePlaceHolder(object sender, EventArgs e)
        {
           var current = (RichTextBox)sender;
           if (current == polynomial1Text && current.Text == "")
           {
               current.Text = "Polynomial1";
               LoadColorFont(current);
           }
           else if (current == polynomial2Text && current.Text == "")
           {
               current.Text = "Polynomial2";
               LoadColorFont(current);
           }
        }
    }
    /// <summary>
    /// Class that holds the history item, each instance holds one record of operation and two polynomials
    /// The record can hold one operation and its roots.
    /// </summary>
    public class HistoryLog
    {
        public Polynomial firstPolynomial, secondPolynomial, resultPolynomial;
        public List<Complex> roots;
        public char Operation { get; private set; }
        public string _timeStamp { get; private set; }
        public string DisplayName { get; set; }
        public HistoryLog returnType { get { return this; } }
        public HistoryLog(Polynomial polynomial1, Polynomial polynomial2, char operation, string timeStamp, Polynomial res)
        {
            this.firstPolynomial = polynomial1;
            this.secondPolynomial = polynomial2;
            this.resultPolynomial = res;
            this.Operation = operation;
            this._timeStamp = timeStamp;
            DisplayName = DateTime.Parse(_timeStamp).ToShortTimeString();
        }
        public HistoryLog(Polynomial polynomial1, List<Complex> roots, string timeStamp)
        {
            this.firstPolynomial = polynomial1;
            this.Operation = '=';
            this.roots = roots;
            this._timeStamp = timeStamp;
            DisplayName = DateTime.Parse(_timeStamp).ToShortTimeString();
        }
    }

}
