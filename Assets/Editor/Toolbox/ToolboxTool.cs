using UnityEngine;

public abstract class ToolboxTool
{
    public virtual string Name => "Tool";


    private PlayerController _pc;
    protected PlayerController pc => _pc;

    public void init(PlayerController pc)
    {
        this._pc = pc;
        init();
    }
    protected abstract void init();

    public void initPlayMode(PlayerController pc)
    {
        this._pc = pc;
        initPlayMode();
    }
    protected abstract void initPlayMode();

    public abstract void display();
}
