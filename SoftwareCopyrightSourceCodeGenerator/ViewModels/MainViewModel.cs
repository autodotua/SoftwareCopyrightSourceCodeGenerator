using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareCopyrightSourceCodeGenerator.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string addingFileOrFolder;

    [ObservableProperty]
    private ObservableCollection<string> files;

    [ObservableProperty]
    private bool isEnable = true;

    [ObservableProperty]
    private int lineCount;

    [ObservableProperty]
    private string results;
    internal void AddFilesOrFolder(IEnumerable<string> files)
    {
        if (Files == null)
        {
            Files = new ObservableCollection<string>();
        }
        foreach (var fileOrFolder in files)
        {
            if (File.Exists(fileOrFolder))
            {
                Files.Add(fileOrFolder);
            }
            else if (Directory.Exists(fileOrFolder))
            {
                foreach (var subFile in Directory.EnumerateFiles(fileOrFolder, "*", SearchOption.AllDirectories))
                {
                    Files.Add(subFile);
                }
            }
        }
    }

    private static bool IsBinary(string filePath, int requiredConsecutiveNul = 3)
    {
        const int charsToCheck = 800;
        const char nulChar = '\0';

        int nulCount = 0;

        using var streamReader = new StreamReader(filePath);
        for (var i = 0; i < charsToCheck; i++)
        {
            if (streamReader.EndOfStream)
            {
                return false;
            }

            if ((char)streamReader.Read() == nulChar)
            {
                nulCount++;
                if (nulCount >= requiredConsecutiveNul)
                {
                    return true;
                }
            }
            else
            {
                nulCount = 0;
            }
        }

        return false;
    }

    [RelayCommand]
    private void AddFilesOrFolder()
    {
        AddFilesOrFolder([AddingFileOrFolder]);
    }

    /// <summary>
    /// 根据文件尝试返回字符编码
    /// </summary>
    /// <param name="file">文件路径</param>
    /// <param name="defEnc">没有BOM返回的默认编码</param>
    /// <returns>如果文件无法读取，返回null。否则，返回根据BOM判断的编码或者缺省编码（没有BOM）。</returns>
    static Encoding GetEncoding(string file, Encoding defEnc)
    {
        using (var stream = File.OpenRead(file))
        {
            //判断流可读？
            if (!stream.CanRead)
                return null;
            //字节数组存储BOM
            var bom = new byte[4];
            //实际读入的长度
            int readc;

            readc = stream.Read(bom, 0, 4);

            if (readc >= 2)
            {
                if (readc >= 4)
                {
                    //UTF32，Big-Endian
                    if (CheckBytes(bom, 4, 0x00, 0x00, 0xFE, 0xFF))
                        return new UTF32Encoding(true, true);
                    //UTF32，Little-Endian
                    if (CheckBytes(bom, 4, 0xFF, 0xFE, 0x00, 0x00))
                        return new UTF32Encoding(false, true);
                }
                //UTF8
                if (readc >= 3 && CheckBytes(bom, 3, 0xEF, 0xBB, 0xBF))
                    return new UTF8Encoding(true);

                //UTF16，Big-Endian
                if (CheckBytes(bom, 2, 0xFE, 0xFF))
                    return new UnicodeEncoding(true, true);
                //UTF16，Little-Endian
                if (CheckBytes(bom, 2, 0xFF, 0xFE))
                    return new UnicodeEncoding(false, true);
            }

            return defEnc;
        }
    }

    //辅助函数，判断字节中的值
    static bool CheckBytes(byte[] bytes, int count, params int[] values)
    {
        for (int i = 0; i < count; i++)
            if (bytes[i] != values[i])
                return false;
        return true;
    }
    [RelayCommand]
    private async void Generate()
    {
        try
        {
            IsEnable = false;
            LineCount = 0;
            StringBuilder str = new StringBuilder();
            await Task.Run(() =>
            {
                foreach (var file in Files)
                {
                    if (IsBinary(file))
                    {
                        return;
                    }
                    var encoding = GetEncoding(file, Encoding.UTF8);
                    foreach (var line in File.ReadLines(file, encoding))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            str.AppendLine(line);
                            LineCount++;
                        }
                    }
                }
            });
            Results = str.ToString();
        }
        catch(Exception ex)
        {
            Results = ex.ToString();
        }
        finally
        {
           IsEnable = true;
        }
    }
}
