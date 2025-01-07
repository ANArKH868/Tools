*存放各类工具，下方介绍工具使用*
***
### ExcelToUnityAsset
*简述：Excel导出为Csv，为Csv创建对应的Cs脚本，通过脚本导出Asset文件*

* 转CSV（忽略ignore列导出）.xlsm <br>
    配合Excel数据文件使用，内部有3个宏：<br>
   >1、设定要存放导出CSV的文件夹地址 <br>
    2、设定要读取Excel的文件夹地址 <br>
    3、保存为CSV <br>

     Excel数据文件，格式参考文件 `表格模板.xlsx`

* GenerateCsvToAsset.cs <br>
    与Excel数据文件一起放入Unity工程文件【Editor】文件夹内，使用前需保证以下几点的路径正确（可在脚本内自行更改地址） <br>
   >1、Csv的存放地址 <br>
    2、Excel对应的Cs脚本地址 <br>
    3、最终转化为Asset文件的存放地址 <br>
    
    上述地址正确后，可通过该脚本的2个方法实现数据存入Asset文件（菜单栏-Tools） <br>
   >1、单个导出，选择对应数据的Cs脚本，并对其转化 <br>
    2、批量导出，显示所有数据有关的Cs脚本，并依次转化 <br>
    
    每个Excel数据文件需要创建一个cs脚本来读取，格式参考文件 `Cs脚本模板.cs`
