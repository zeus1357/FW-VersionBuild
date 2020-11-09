using System;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace FW_VersionBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n##### UNIERP FW Infragistics 버전별 빌드를 위한 sln 및 csproj 파일 자동 작성기입니다. #####");
            Console.Write("\n 1. Infragistics 버전을 입력해 주세요 : ");
            string input_infragistics_ver = Console.ReadLine();
            
            Console.WriteLine("\n 2. UNIERP V5.6 AppFramework 소스폴더 위치를 입력해 주세요");
            Console.WriteLine("    기본경로 (D:\\BIZENTRO.NET\\Bizentro\\Frameworks\\AppFramework) ");
            Console.WriteLine("    아무값도 입력하지 않으면 기본경로로 자동 설정됩니다. ");
            Console.Write("\n    AppFramework 소스폴더 위치를 입력 : ");
            string input_Appframework_Folder_Path = Console.ReadLine();

            if (input_Appframework_Folder_Path.Equals(""))
                input_Appframework_Folder_Path = @"D:\BIZENTRO.NET\Bizentro\Frameworks\AppFramework";
            
            Console.WriteLine($"\n (설명) {input_Appframework_Folder_Path} 폴더 아래 모든 프로젝트 폴더에"); 
            Console.WriteLine($"        기존 프로젝트명.iv{input_infragistics_ver}.csproj 파일 및 Bizentro.net.{input_infragistics_ver}.sln 파일이 생깁니다.");
            Console.WriteLine($"        .csproj 파일의 빌드 이벤트 중 파일복사 경로를  AppFrameworks.{input_infragistics_ver} 폴더로 수정합니다.");

            
            ProjectFileSearch(input_Appframework_Folder_Path, input_infragistics_ver);    

            Console.Write("\nPress any key to exit...");
            Console.ReadKey(true);
        }

        static void ProjectFileSearch(string path, string iv_ver)
        {
            string[] Appframwork_sub_dirs = Directory.GetDirectories(path);
            string[] Appframwork_sub_dir_name;
            string project_folder_name = "";
            string project_file_name = "";
            string new_project_file_name = "";
            string infragistics_referenced_project_file_path = System.Environment.CurrentDirectory + @"\InfragisticsReferencedProject.txt";
            
            string[] all_lines = System.IO.File.ReadAllLines(infragistics_referenced_project_file_path);
            List<string> infragistics_referenced_projects_list = new List<string>(all_lines);

            //string project_file_path = @"D:\BIZENTRO.NET\Bizentro\Frameworks\AppFramework\Bizentro.AppFramework.UI.Controls\Bizentro.AppFramework.UI.Controls.iv20.1.112.csproj";               
            
            string replace_word = iv_ver;


            foreach(string d in Appframwork_sub_dirs)
            {
                if(d.Contains(".dm"))
                    continue;

                string[] Appframwork_sub_dir_phrase = d.Split('\\');
                project_folder_name = Appframwork_sub_dir_phrase[5].ToString();
                //project_folder_name : Bizentro.AppFramework.AssemblyLoadingIntercepter
                project_file_name = project_folder_name + ".csproj";
                new_project_file_name = project_folder_name + ".iv" + iv_ver + ".csproj";
                //new_project_file_name : Bizentro.AppFramework.AssemblyLoadingIntercepter.iv20.1.12.csproj

                foreach(string f in Directory.GetFiles(d,project_file_name))
                {
                    File.Copy(Path.Combine(d,project_file_name), Path.Combine(d,new_project_file_name));
                    string project_file_path = Path.Combine(d,new_project_file_name);

                    NewProjectFileModify(infragistics_referenced_projects_list, project_file_path, replace_word);
                }
            }    
        }

        static void NewProjectFileModify(List<string> ir_projects_list, string project_file_path, string replace_word)
        {
            //string project_file_path = @"D:\BIZENTRO.NET\Bizentro\Frameworks\AppFramework\Bizentro.AppFramework.UI.Controls\Bizentro.AppFramework.UI.Controls.iv20.1.112.csproj";
            string[] all_lines = System.IO.File.ReadAllLines(project_file_path);
            //string replace_word = "20.1.12";
            int index = 0;

            foreach(string l in all_lines)
            {
                // infragistics 참조버전 변경
                //<Reference Include="Infragistics.Documents.Excel, Version=20.1.20201.12, Culture=neutral, PublicKeyToken=7dd5c3163f2cd0cb, processorArchitecture=MSIL" />
                if (l.Contains("Infragistics"))
                {
                    if(l.Contains("Version="))
                    {
                        //Console.WriteLine(l);
                        int _first = l.IndexOf("Version=");
                        int first_location = _first + 8;
                        int last_location = l.IndexOf(", ", first_location);
                        int replace_word_count = last_location - first_location;
                        
                        string before_replace_word = l.Substring(first_location, replace_word_count);
                        all_lines[index] = l.Replace(before_replace_word, replace_word);
                        //Console.WriteLine(all_lines[index].ToString());
                    }                    

                }
                
                // UNIERP FW 참조 변경
                /*
                <ProjectReference Include="..\Bizentro.AppFramework.UI.Providers\Bizentro.AppFramework.UI.Providers.iv20.1.20201.12.csproj">
                    <Project>{23829BD8-69C9-4A2D-AD4B-E907FE269D8B}</Project>
                    <Name>Bizentro.AppFramework.UI.Providers.iv20.1.20201.12</Name>
                </ProjectReference>
                */
                int index2 = 0;
                foreach(string t in ir_projects_list)
                {
                    if ( l.Contains(t) )
                    {
                        if (l.Contains("<ProjectReference"))    //<ProjectReference Include="..\Bizentro.AppFramework.UI.Common\Bizentro.AppFramework.UI.Common.csproj">
                        {
                            //Console.WriteLine(l.ToString());
                            int _first = l.LastIndexOf("\\") + 1;
                            int _last = l.LastIndexOf(".csproj");
                            int replace_word_count = _last - _first;

                            string before_replace_word = l.Substring(_first, replace_word_count);
                            string _temp = l.Replace(before_replace_word+".csproj", before_replace_word + ".iv." + replace_word + ".csproj");
                            Console.WriteLine(_temp);
                        
                        }else if (l.Contains("<Name>"))  //<Name>Bizentro.AppFramework.UI.Common</Name>    
                        {
                            //Console.WriteLine(l.ToString());    
                            int _first = l.IndexOf("<Name>");
                            int first_location = _first + 6;
                            int last_location = l.IndexOf("</Name>", first_location);
                            
                            int replace_word_count = last_location - first_location;

                            string before_replace_word = l.Substring(first_location, replace_word_count);
                            string _temp = l.Replace(before_replace_word, before_replace_word + ".iv." + replace_word);
                            Console.WriteLine(_temp);

                        }else{}
                    }   //if ( l.Contains(t) )
                    index2++;
                }   //foreach(string t in ir_projects_list)
                index++;
            }   //foreach(string l in all_lines)
            System.IO.File.WriteAllLines(project_file_path , all_lines);

        }   //static void NewProjectFileModify(List<string> ir_projects_list, string project_file_path, string replace_word)  
        
    }
}
