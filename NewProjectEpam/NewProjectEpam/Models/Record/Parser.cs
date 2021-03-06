﻿using ParserInterfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CatalogAppMVC.Models
{
    public class Parser
    {
        //Parser.ParseSite(Assembly.LoadFrom("D:\\parser_infofrezer_ru.dll"));
        IParser parser;
        Type parserType;
        Type parserRecordType;

        public static void ParserStart(HttpContextBase httpContext)
        {
            Parser parser = new Parser();
            string p = ConfigurationManager.AppSettings["parser"];
            string patch = httpContext.Server.MapPath(p);
            parser.ParseSite(Assembly.LoadFrom(patch), httpContext);
        }

        //TODO Добавить записи в логи
        public void ParseSite(Assembly parserDLL, HttpContextBase http)
        {
            try
            {
                parserType = (from p in parserDLL.GetTypes() where p.IsClass && (p.GetInterface("IParser") != null) select p).First();
                parserRecordType = (from p in parserDLL.GetTypes() where p.IsClass && (p.GetInterface("IRecord") != null) select p).First();

                parser = (IParser)parserDLL.CreateInstance(parserType.FullName, true);
            }
            catch
            {
                //Запись в лог - подключена некорректная библиотека
                return;
            }

            Dictionary<string, string> categoriesFromSite;
            try
            {
                categoriesFromSite = parser.GetCategories();
            }
            catch(Exception ex)
            {
                //запись в лог ex.Message
                return;
            }

            //List<Category> categories = Category.GetAllCategory();
            List<Category> categories = Category.GetAllCategory();
            foreach(KeyValuePair<string, string> categorySite in categoriesFromSite)
            {
                Category category = CategoryInBase(categorySite.Key, categories);
                if(category != null)
                {
                    LoadNewRecords(categorySite.Value, category, http);
                }
                else
                {
                    //Запись в лог о том, что найдена новая категория
                    //записи из новой категории не добавляются
                    //Возможно вывести предложение создать новую категорию
                }
            }

        }

        Category CategoryInBase(string categoryName, List<Category> categoriesInBase)
        {
            foreach(Category category in categoriesInBase)
            {
                if (category.Name.Equals(categoryName, StringComparison.CurrentCultureIgnoreCase))
                    return category;
            }
            return null;
        }


        void LoadNewRecords(string categoryURL, Category category, HttpContextBase http)
        {
            Dictionary<string, string> recordsFromSite;
            try
            {
                recordsFromSite = parser.GetRecordsFromCategory(categoryURL);
            }
            catch(Exception ex)
            {
                //запись в лог ex.Message
                return;
            }
            List<Record> allRecord = Record.GetAllRecords();
            foreach(KeyValuePair<string, string> recordFromSite in recordsFromSite)
            {
                if (!RecordInBase(recordFromSite.Key, allRecord))
                {
                    Record recordNew = LoadRecord(recordFromSite.Value, category, http);
                    if (recordNew != null)
                    {
                        recordNew.AddToDataBase();
                        int recordID = recordNew.ID;
                        foreach(File file in recordNew.Files)
                        {
                            file.RecordID = recordID;
                            file.AddToBase();
                        }
                    }
                }
            }
        }
        bool RecordInBase(string nameNewRecord, List<Record> allRecords)
        {
            foreach (Record record in allRecords)
            {
                if (record.Name.Equals(nameNewRecord, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }
        Record LoadRecord(string recordURL, Category category, HttpContextBase http)
        {
            IRecord recordNew;
            try
            {
                recordNew = parser.GetRecord(recordURL);
            }
            catch(Exception ex)
            {
                //запись в лог ex.Message
                return null;
            }
            Record record = new Record();
            record.Name = recordNew.Name;
            record.Description = recordNew.Description;
            record.CategoryID = category.ID;
            record.UserAuthorID = Access.ADMINID;

            record.Specifications = new List<Specification>();
            foreach(KeyValuePair<string, string> specificationNew in recordNew.Specifications)
            {
                record.Specifications.Add(new Specification() { Name = specificationNew.Key, Value = specificationNew.Value });
            }

            record.Files = new List<File>();
            if (recordNew.PhotoHeadUrl != null)
            {
                //загрузка файла и сохранение в базе как аватарки
            }

            if (recordNew.Files != null)
            {
                foreach(KeyValuePair<string, string> fileNew in recordNew.Files)
                {
                    File file = null;
                    if((file = File.CreateFileLink(fileNew.Value, fileNew.Key, "/FromSite/", http)) != null)
                    {
                        record.Files.Add(file);
                    }
                }
            }


            return record;
        }
    }
}