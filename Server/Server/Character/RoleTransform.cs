
using Luna3D;

public class Transform
{
    public string name;

    public Vector3 position;

    public Quaternion rotation;

    public Vector3 localScale;
}


public class RoleTransform 
{
    private Transform _trans;

    public RoleTransform()
    {
        _trans = new Transform();
    }

    public Transform GetTransform()
    {
        return _trans;
    }

    public string GetTranName()
    {
        return _trans.name;
    }

    public void SetTranName(string name)
    {
        _trans.name = name;
    }

    public Vector3 GetPosition()
    {
        return _trans.position;
    }

    public void SetPosition(Vector3 pos)
    {
        _trans.position = pos;
    }

    public Vector3 GetLocalScale()
    {
        return _trans.localScale;
    }

    public void SetLocalScale(Vector3 scale)
    {
        _trans.localScale = scale;
    }

    public Quaternion GetRotation()
    {
        return _trans.rotation;
    }

    public void SetRotation(Quaternion quat)
    {
        _trans.rotation = quat;
    }
}