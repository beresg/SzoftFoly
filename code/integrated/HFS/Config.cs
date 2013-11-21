using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace HFS
{
    public class Config
    {
        public int Port { get; set; }
        public String Name { get; set; }
        public Boolean AllowUpload { get; set; }
        public int MaxUsers { get; set; }
    }

    public class ConfigAdapter
    {
        static public BindingList<Config> load()
        {
            BindingList<Config> res = new BindingList<Config>();

            string[] strings = null;

            try
            {
                strings = File.ReadAllLines("settings.hfs");
            }
            catch (Exception e)
            {
            }

            if (strings != null)
            {
                string[] items = null;

                foreach (string item in strings)
                {
                    items = null;

                    items = item.Split('|');

                    if (items != null && items.Length == 4)
                    {
                        int port;
                        int maxUsers;
                        bool allow;

                        if (Int32.TryParse(items[1], out port) &&
                            Int32.TryParse(items[2], out maxUsers) &&
                            Boolean.TryParse(items[3], out allow))
                        {
                            res.Add(new Config() {
                                Name = items[0],
                                Port = port,
                                MaxUsers = maxUsers,
                                AllowUpload = allow
                            });
                        }
                    }
                }
            }

            return res;
        }

        static public bool save(BindingList<Config> list)
        {
            // Write a string array to a file.
            List<string> strings = new List<string>();

            foreach (Config item in list)
                strings.Add(item.Name + "|" + item.Port + "|" + item.MaxUsers + "|" + item.AllowUpload);

            try
            {
                File.WriteAllLines("settings.hfs", strings);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
