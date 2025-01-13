using UnityEngine;

public abstract class ToolboxTool
{
    public virtual string Name => "Tool";


    public PlayerController pc;

    public void init(PlayerController pc)
    {
        this.pc = pc;
        init();
    }
    protected abstract void init();

    public void initPlayMode(PlayerController pc)
    {
        this.pc = pc;
        initPlayMode();
    }
    protected abstract void initPlayMode();

    public abstract void dispose();

    public abstract void display();
}
