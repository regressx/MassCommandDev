namespace OrionMassCommandSenderOld
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Xml;

    public class Loader
    {
            private static Point _tableProp;

    public static bool CheckOptionsFile(string Path)
    {
      XmlDocument xmlDocument = new XmlDocument();
      try
      {
        try
        {
          xmlDocument.Load(Path);
        }
        catch (FileNotFoundException ex)
        {
          Logger.AddLog(string.Format("Не найден файл настроек"));
          return false;
        }
        XmlNodeList xmlNodeList = xmlDocument.SelectNodes("ports/port");
        XmlNode xmlNode = xmlDocument.SelectSingleNode("ports/table");
        if (xmlNodeList.Count > 0 & xmlNode != null)
        {
          if (xmlNode.Attributes[0].Name == "Columns" & xmlNode.Attributes[1].Name == "Rows")
          {
            for (int index = 0; index < xmlNodeList.Count; ++index)
            {
              if (xmlNodeList[index].Attributes[0].Name != "name" | xmlNodeList[index].Attributes[1].Name != "baud")
              {
                Logger.AddLog(string.Format("Ошибка в строке {0} списка устройств: отсутствует один или оба атрибута", (object) (index + 1)));
                return false;
              }
              if (xmlNodeList[index].Attributes[0].Value == string.Empty | xmlNodeList[index].Attributes[1].Value == string.Empty)
              {
                Logger.AddLog(string.Format("Ошибка в строке {0} списка устройств: есть пустые значения в атрибутах", (object) (index + 1)));
                return false;
              }
            }
            try
            {
              if (int.Parse(xmlNode.Attributes[1].Value) * int.Parse(xmlNode.Attributes[0].Value) != xmlNodeList.Count)
              {
                Logger.AddLog("Невозможно построить требуемую таблицу. Не совпадают значения количества устройств с количеством ячеек таблицы");
                return false;
              }
            }
            catch (ArgumentNullException ex)
            {
              Logger.AddLog("Пустое значение в атрибуте узла table");
              return false;
            }
            catch (FormatException ex)
            {
              Logger.AddLog("Неверный формат данных в атрибуте узла table");
              return false;
            }
            return true;
          }
        }
        else
          Logger.AddLog("Не найден один из узлов или оба узла файла настроек. Пересоздайте его.");
      }
      catch (FileNotFoundException ex)
      {
        Logger.AddLog("Файл настроек не найден");
      }
      catch (Exception ex)
      {
        Logger.AddLog("Произошла ошибка при проверке файла настроек. Пересоздайте файл с нужной структурой через окно настройки портов");
      }
      finally
      {
        xmlDocument.RemoveAll();
      }
      return false;
    }

    public static DataTable ReadOptions(string Path)
    {
      if (!Loader.CheckOptionsFile(Path))
        return (DataTable) null;
      DataTable dataTable = new DataTable();
      XmlDocument xmlDocument = new XmlDocument();
      xmlDocument.Load(Path);
      XmlNodeList xmlNodeList = xmlDocument.SelectNodes("ports/port");
      XmlNode xmlNode = xmlDocument.SelectSingleNode("ports/table");
      Loader._tableProp.Y = int.Parse(xmlNode.Attributes[1].Value);
      Loader._tableProp.X = int.Parse(xmlNode.Attributes[0].Value);
      DataColumn column1 = new DataColumn("Name", Type.GetType("System.String"));
      DataColumn column2 = new DataColumn("baud", Type.GetType("System.Int32"));
      dataTable.Columns.Add(column1);
      dataTable.Columns.Add(column2);
      for (int index = 0; index < xmlNodeList.Count; ++index)
      {
        DataRow row = dataTable.NewRow();
        row[0] = (object) xmlNodeList[index].Attributes[0].Value;
        row[1] = (object) int.Parse(xmlNodeList[index].Attributes[1].Value);
        dataTable.Rows.Add(row);
      }
      return dataTable;
    }

    public static Point TableProperties
    {
      get
      {
        return Loader._tableProp;
      }
    }

    public static void CreateOptionsFile()
    {
    }
    }
}