*存放各类工具，下方介绍工具使用*
***
### ExcelToUnityAsset
*简述：Excel导出为Csv，为Csv创建对应的Cs脚本，通过脚本导出Asset文件*

* 转CSV（忽略ignore列导出）.xlsm <br>
    配合Excel数据文件使用，内部有3个宏：<br>
   >1、设定要存放导出CSV的文件夹地址 <br>
    2、设定要读取Excel的文件夹地址 <br>
    3、保存为CSV <br>

     Excel数据文件的表头参考如下 <br>
     ```
     第一行：中文说明
     第二行：二级分组（取自字段类型为List<XXX>中的XXX）
     第三行：字段名
     第四行：字段类型
     第五行：数据内容
     ```

* GenerateCsvToAsset.cs <br>
    与Excel数据文件一起放入Unity工程文件【Editor】文件夹内，使用前需保证以下几点的路径正确（可在脚本内自行更改地址） <br>
   >1、Csv的存放地址 <br>
    2、Excel对应的Cs脚本地址 <br>
    3、最终转化为Asset文件的存放地址 <br>
    
    上述地址正确后，可通过该脚本的2个方法实现数据存入Asset文件（菜单栏-Tools） <br>
   >1、单个导出，选择对应数据的Cs脚本，并对其转化 <br>
    2、批量导出，显示所有数据有关的Cs脚本，并依次转化 <br>
    
    每个Excel数据文件需要创建一个cs脚本来读取，格式如下
    ```
    using System.Collections.Generic;
    using UnityEngine;

    public class 类名与Excel名称一致 : ScriptableObject
    {
      /// <summary>
      /// 根据ID获取list所在的index标记位
      /// </summary>
      public Dictionary<int, int> idStorageDic = new Dictionary<int, int>();
  
      public List<自定义一级类名> 自定义一级List = new List<自定义一级类名>();
  
      public void AddList(int id, 自定义一级List data)
      {
          data.count = new();  // 无二级分组忽略此行代码
          自定义一级List.Add(data);
          idStorageDic.Add(id, 自定义一级List.Count - 1);
      }
  
      // 无二级分组忽略此方法
      public void SecondAddList(int id, 二级分组类 secData)
      {
          自定义一级List[idStorageDic[id]].count.Add(secData);
      }
    }

    [System.Serializable]
    public class 自定义一级类名
    {
        //公开 字段类型 字段名（与Excel写的一致）
        public int id;
      
        //公开 二级分组的字段类型List<XXX> 字段名
        public List<EquipCount> count;
    }
  
    // 存在二级分组时添加该类（类名与Excel二级分组名一致）
    [System.Serializable]
    public class 二级分组类
    {
        //公开 字段类型 字段名（与Excel写的一致）
        public int value;
    }
    ```
