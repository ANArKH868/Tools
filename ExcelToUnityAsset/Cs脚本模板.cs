using System.Collections.Generic;
using UnityEngine;

public class ��Csv�ļ���һ�� : ScriptableObject
{
    /// <summary>
    /// ����ID��ȡlist���ڵ�index���λ
    /// </summary>
    public Dictionary<int, int> idStorageDic = new Dictionary<int, int>();
    public List<�Զ���һ����> һ��List = new List<�Զ���һ����>();

    public void AddList(int id, �Զ���һ���� data)
    {
        data.XXX = new();  // �޶���������Դ��д���
        һ��List.Add(data);
        idStorageDic.Add(id, һ��List.Count - 1);
    }

    // �޶���������Դ˷���
    public void SecondAddList(int id, ���������� secData)
    {
        һ��List[idStorageDic[id]].XXX.Add(secData);
    }
}

[System.Serializable]
public class �Զ���һ����
{
	//���� �ֶ����� �ֶ�������Excelд��һ�£�
    public int id;
    public List<����������> XXX;
}

// ���ڶ�������ʱ��Ӹ��ࣨ������Excel����������һ�£�
[System.Serializable]
public class ����������
{
    //���� �ֶ����� �ֶ�������Excelд��һ�£�
    public int value;
}
