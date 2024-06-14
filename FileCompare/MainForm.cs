using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FileCompare.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using Color = DocumentFormat.OpenXml.Spreadsheet.Color;
using Font = DocumentFormat.OpenXml.Spreadsheet.Font;

namespace FileCompare
{
    public partial class MainForm : Form
    {
        private delegate void DelegateSetFormEnable(bool isEnable);
        private delegate void DelegateSetList();
        private delegate void DelegateSetLblPro(string lblText);

        private ConcurrentDictionary<string, (long size, string hash)> fileHashes1;
        private ConcurrentDictionary<string, (long size, string hash)> fileHashes2;

        private FileInfoDB fileInfoDBcontext = new FileInfoDB();
        private List<FileContext> FileContextList;
        private string reportFile;


        public MainForm()
        {
            InitializeComponent();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(BgWorker_ProgressChanged);
            panel1.Visible = false;
            txtPath1.Text = @"H:\";
            txtPath2.Text = @"I:\";
        }



        private void MainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(fileInfoDBcontext.DbPath))
            {
                fileInfoDBcontext.Database.ExecuteSqlRaw("DELETE FROM FileContext");
            }
            reportFile = Path.Combine(Path.GetDirectoryName(fileInfoDBcontext.DbPath), "Report.xlsx");
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Process was cancelled", "Process Cancelled");
            }
            SetFormEnableInTask(true);
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = reportFile,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open file: {ex.Message}");
            }
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                SetFormEnableInTask(false);
                string folderPath1 = @txtPath1.Text;
                string folderPath2 = @txtPath2.Text;

                SetLblProInTask("Get file information...");
                bgWorker.ReportProgress(0);
                List<string> folderList = GetParentPathOrPaths(folderPath1, folderPath2);
                SetFileHashToDB(folderList);

                SetLblProInTask("Create same file list...");
                bgWorker.ReportProgress(0);
                FileContextList = fileInfoDBcontext.FileContext.OrderBy(x => x.FileMd5).ToList();
                SaveToExcel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            FileContextList = new();
            if (bgWorker.IsBusy != true)
            {
                bgWorker.RunWorkerAsync();
            }
        }

        private void SetFormEnableInTask(bool isEnable)
        {
            try
            {
                DelegateSetFormEnable delegateSetFormEnable = new DelegateSetFormEnable(SetFormEnable);
                this.Invoke(delegateSetFormEnable, isEnable);
            }
            catch (Exception)
            {
            }
        }

        private void SetFormEnable(bool isEnable)
        {
            foreach (System.Windows.Forms.Control control in Controls)
            {
                if (control != panel1)
                {
                    control.Enabled = isEnable;
                }
            }
            panel1.Visible = !isEnable;
        }

        private void SetLblProInTask(string lblText)
        {
            try
            {
                DelegateSetLblPro delegateSetLblPro = new DelegateSetLblPro(SetLblPro);
                this.Invoke(delegateSetLblPro, lblText);
            }
            catch (Exception)
            {
            }
        }

        private void SetLblPro(string lblText)
        {
            lblPro.Text = lblText;
        }

        private List<string> GetParentPathOrPaths(string path1, string path2)
        {
            string fullPath1 = Path.GetFullPath(path1).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string fullPath2 = Path.GetFullPath(path2).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            var result = new List<string>();

            if (fullPath2.StartsWith(fullPath1, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(fullPath1);
            }
            else if (fullPath1.StartsWith(fullPath2, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(fullPath2);
            }
            else
            {
                result.Add(fullPath1);
                result.Add(fullPath2);
            }

            return result;
        }

        private void SetFileHashToDB(List<string> folderList)
        {
            var fileList = new List<string>();

            try
            {
                foreach (var folderPath in folderList)
                {
                    foreach (var file in EnumerateFilesSafe(folderPath))
                    {
                        fileList.Add(file);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to folder: {ex.Message}");
            }

            int totalFile = 0;
            foreach (var filePath in fileList)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);
                    string hash = ComputeMD5(filePath);
                    FileContext fileContext = new()
                    {
                        FolderPath = Path.GetDirectoryName(filePath),
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        FileMd5 = hash,
                        FileSize = fileInfo.Length
                    };

                    var existingEntity = fileInfoDBcontext.FileContext.SingleOrDefault(e => e.FilePath == fileContext.FilePath);
                    if (existingEntity != null)
                    {
                        fileInfoDBcontext.FileContext.Remove(existingEntity);
                    }
                    fileInfoDBcontext.FileContext.Add(fileContext);

                    totalFile++;
                    int progress = (int)(((double)(totalFile) / fileList.Count) * 100);
                    bgWorker.ReportProgress(progress);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Access denied to file: {filePath}");
                }
            }
            fileInfoDBcontext.SaveChanges();
        }

        private void SaveToExcel()
        {
            if (!File.Exists(reportFile))
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(reportFile, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());
                    WorkbookStylesPart stylePart = workbookPart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = CreateStylesheet();
                    stylePart.Stylesheet.Save();
                    Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
                    sheets.Append(sheet);

                    SetSheetHeader(worksheetPart);
                    SetSheetContent(worksheetPart);

                    // Save changes to the document
                    workbookPart.Workbook.Save();
                }
            }
            else
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(reportFile, true))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                    WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    newWorksheetPart.Worksheet = new Worksheet(new SheetData());
                    uint newSheetId = workbookPart.Workbook.Sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
                    Sheet newSheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(newWorksheetPart), SheetId = newSheetId, Name = $"Sheet{newSheetId}" };
                    sheets.Append(newSheet);
                    SetSheetHeader(newWorksheetPart);
                    SetSheetContent(newWorksheetPart);

                    // Save changes to the document
                    workbookPart.Workbook.Save();
                }
            }
        }

        private Stylesheet CreateStylesheet()
        {
            Fonts fonts = new Fonts(
                new Font(new FontSize() { Val = 12 }),
                new Font(
                    new FontSize() { Val = 16 },
                    new Bold(),
                    new Color() { Rgb = "000000" }
                )
            );
            Fills fills = new Fills(
                new Fill(),
                new Fill(
                    new PatternFill(
                        new ForegroundColor()
                        {
                            Rgb = new HexBinaryValue() { Value = "FFFFFF" }
                        }
                    )
                    { PatternType = PatternValues.Solid }),
                new Fill(
                    new PatternFill(
                        new ForegroundColor()
                        {
                            Rgb = new HexBinaryValue() { Value = "D3D3D3" }
                        }
                    )
                    { PatternType = PatternValues.Solid })
            );
            Borders borders = new Borders(
                new Border(),
                new Border(
                    new LeftBorder(new Color() { Auto = true })
                    {
                        Style = BorderStyleValues.Thin
                    },
                    new RightBorder(new Color() { Auto = true })
                    {
                        Style = BorderStyleValues.Thin
                    },
                    new TopBorder(new Color() { Auto = true })
                    {
                        Style = BorderStyleValues.Thin
                    },
                    new BottomBorder(new Color() { Auto = true })
                    {
                        Style = BorderStyleValues.Thin
                    },
                    new DiagonalBorder())
            );
            CellFormats cellFormats = new CellFormats(
                new CellFormat { FontId = 1, FillId = 1, BorderId = 1 },
                new CellFormat { BorderId = 1 },
                new CellFormat { FontId = 1, FillId = 2, BorderId = 1 }
            );
            return new Stylesheet(fonts, fills, borders, cellFormats);
        }

        private void SetSheetHeader(WorksheetPart worksheetPart)
        {
            string[] headers = { "GroupID", "FileName", "CMD", "Path", "CMD" };
            char startColumn = 'B';
            uint startRow = 2;

            for (int i = 0; i < headers.Length; i++)
            {
                string cellReference = $"{(char)(startColumn + i)}";
                SetCellValue(worksheetPart, cellReference, startRow, 2, headers[i]);
            }
        }

        private void SetSheetContent(WorksheetPart worksheetPart)
        {
            int totalFile = 0;
            string cellReference = string.Empty;
            string tempGroup = string.Empty;
            int groupID = 0;
            uint startRow = 3;
            foreach (var file in FileContextList)
            {
                totalFile++;
                int progress = (int)(((double)(totalFile) / FileContextList.Count) * 100);
                bool hasDel = false;
                if (tempGroup != file.FileMd5)
                {
                    tempGroup = file.FileMd5;
                    groupID++;
                    hasDel = true;
                }

                char startColumn = 'B';
                cellReference = $"{(char)(startColumn++)}";
                SetCellValue(worksheetPart, cellReference, startRow, 1, $"{groupID}");
                cellReference = $"{(char)(startColumn++)}";
                SetCellValue(worksheetPart, cellReference, startRow, 1, file.FileName);
                cellReference = $"{(char)(startColumn++)}";
                SetCellValue(worksheetPart, cellReference, startRow, 1, hasDel ? "" : "del \"");
                cellReference = $"{(char)(startColumn++)}";
                SetCellValue(worksheetPart, cellReference, startRow, 1, file.FilePath);
                cellReference = $"{(char)(startColumn++)}";
                SetCellValue(worksheetPart, cellReference, startRow, 1, "\"");
                startRow++;
                bgWorker.ReportProgress(progress);
            }
        }

        public Cell SetCellValue(WorksheetPart worksheetPart, string columnName, uint rowIndex, uint styleIndex, string text)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();

            Row row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);
            if (row == null)
            {
                row = new Row { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            string cellReference = columnName + rowIndex;
            Cell cell = row.Elements<Cell>().FirstOrDefault(c => c.CellReference.Value == cellReference);
            if (cell != null)
                return cell;
            Cell refCell = row.Elements<Cell>()
                .FirstOrDefault(c => string.Compare(c.CellReference.Value, cellReference, true) > 0);
            Cell newCell = new Cell()
            {
                CellReference = cellReference
            };
            row.InsertBefore(newCell, refCell);

            newCell.CellValue = new CellValue(text);
            newCell.DataType = new EnumValue<CellValues>(CellValues.String);
            newCell.StyleIndex = styleIndex;
            return newCell;
        }

        private IEnumerable<string> EnumerateFilesSafe(string path)
        {
            var files = new List<string>();
            try
            {
                files.AddRange(Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly));
                foreach (var dir in Directory.EnumerateDirectories(path))
                {
                    files.AddRange(EnumerateFilesSafe(dir));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get folder: {path} Error:{ex.Message}");
            }
            return files;
        }

        private string ComputeMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
