using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRC_SS_Butler
{
    public class File_Butler
    {
        private System.Text.RegularExpressions.Regex reg;

        public File_Butler()
        {
            reg = new System.Text.RegularExpressions.Regex(@"[0-9]{4}-[0-9]{2}-[0-9]{2}_[0-9]{2}-[0-9]{2}-[0-9]{2}");
        }

        public void moveFileTo(string sourceFile, string targetPath)
        {
            if (!isFolderExists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            var fileInfo = new System.IO.FileInfo(sourceFile);
            fileInfo.MoveTo(targetPath + @"/" + fileInfo.Name);
        }

        public string isRegMatch(string fileName)
        {
            var match = reg.Match(fileName);
            string dateText = "";
            while (match.Success)
            {
                dateText = match.Value;
                match = match.NextMatch();
            }
            return dateText;
        }

        private bool isFolderExists(string targetPath)
        {
            return Directory.Exists(targetPath);
        }

        public void copyFoluderTo(string sourcePath, string targetPath)
        {
            if (!isFolderExists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
                File.SetAttributes(targetPath, File.GetAttributes(sourcePath));
            }

            if (targetPath[targetPath.Length - 1] != Path.DirectorySeparatorChar)
            {
                targetPath = targetPath + Path.DirectorySeparatorChar;
            } 

            string[] files = Directory.GetFiles(sourcePath);
            foreach (string file in files)
            {
                if (File.Exists(targetPath + Path.GetFileName(file))) continue;
                File.Copy(file, targetPath + Path.GetFileName(file));
            }
                
            string[] dirs = Directory.GetDirectories(sourcePath);
            foreach (string dir in dirs)
            {
                copyFoluderTo(dir, targetPath + Path.GetFileName(dir));
            }
        }

        public string makeFolderName(string dateText)
        {
            var dateTime = DateTime.ParseExact(dateText, "yyyy-MM-dd_HH-mm-ss", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None);
            var timeSpan = new TimeSpan(Properties.Settings.Default.timeChangeLine, 0, 0);
            dateTime = dateTime - timeSpan;
            if (Properties.Settings.Default.folderMakeDays)
            {
                return $@"{dateTime.Year:0000}-{dateTime.Month:00}-{dateTime.Day:00}";
            }
            else
            {
                return $@"{dateTime.Year:0000}年\{dateTime.Month:00}月\{dateTime.Day:00}日";
            }
        }

        public void emptyFoldersRemove(string sourcePath)
        {
            if (System.IO.Directory.EnumerateFileSystemEntries(sourcePath).Any())
            {
                string[] dirs = Directory.GetDirectories(sourcePath);
                foreach (string dir in dirs)
                {
                    emptyFoldersRemove(dir);
                }
            }
            else
            {
                Directory.Delete(sourcePath, false);
            }
        }
    }
}
