﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using PostmanCollectionToPythonRequests.Models;

namespace PostmanCollectionToPythonRequests
{
    class Program
    {
        static void Main(string[] args)
        {


            var files = Directory.GetFiles("./jsons");
            foreach (var file in files)
            {
                Run(file);
            }

            Console.WriteLine("全部执行结束，按回车键退出");
            Console.ReadLine();

        }


        static void Run(string filePath)
        {
            var jsonFile = new FileInfo(filePath);
            var fileContent = jsonFile.OpenText().ReadToEnd();

            var model = PostManJson.FromJson(fileContent);
            var api = new PythonModel
            {
                Name = model.Info.Name,
            };

            if (IsChs(api.Name))
            {
                var firstItem = model.Item.Skip(1).Take(1).FirstOrDefault();
                var path = firstItem?.Request?.Url?.Path;
                if (path?.Count > 0)
                {
                    api.Name = path.FirstOrDefault();
                    api.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(api.Name.ToLower());
                }
            }

            string content = api.BuildClassContent(model.Item);
            api.GenerateClassFile(Path.Combine("python", "RequestServices", api.Name + "Service.py"), api.Name, content);
            api.GenerateTestFile(Path.Combine("python", "ServiceTests", api.Name + "Test.py"), api.Name);

            Console.WriteLine("生成代码成功:" + api.Name);
        }

        static bool IsChs(string text)
        {
            var result = false;
            char[] c = text.ToCharArray();
            for (int i = 0; i < c.Length; i++)
                if (c[i] >= 0x4e00 && c[i] <= 0x9fbb)
                    return true;

            return result;
        }

    }
}
