
using System;

public class ProtoHelper
{
    public static common.Vector Str2ProtoVec(string str)
    {
        string[] array = str.Split(',');
        common.Vector vec = new common.Vector();
        vec.x = Convert.ToSingle(array[0]);
        vec.y = Convert.ToSingle(array[1]);
        vec.z = Convert.ToSingle(array[2]);

        return vec;
    }

    public static Luna3D.Vector3 Str2Vec(string str)
    {
        string[] array = str.Split(',');
        return new Luna3D.Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
    }

    public static Luna3D.Vector3 PV2LV(common.Vector protoVec)
    {
        return new Luna3D.Vector3(protoVec.x, protoVec.y, protoVec.z);
    }

    public static common.Vector LV2PV(Luna3D.Vector3 vec)
    {
        common.Vector ProtoVec = new common.Vector();
        ProtoVec.x = vec.x;
        ProtoVec.y = vec.y;
        ProtoVec.z = vec.z;

        return ProtoVec;
    }

    public static common.Vector UV2PV(UnityEngine.Vector3 vec)
    {
        common.Vector pVec = new common.Vector();
        pVec.x = vec.x;
        pVec.y = vec.y;
        pVec.z = vec.z;
        return pVec;
    }

    public static UnityEngine.Vector3 PV2UV(common.Vector pVec)
    {
        return new UnityEngine.Vector3(pVec.x, pVec.y, pVec.z);
    }
}
