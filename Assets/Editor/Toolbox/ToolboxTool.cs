using UnityEngine;

public abstract class ToolboxTool
{
    public virtual string Name => "Tool";


    public PlayerController pc;

    public void init(PlayerController pc)
    {
        this.pc = pc;
        init();
        load();
    }
    protected abstract void init();

    public void initPlayMode(PlayerController pc)
    {
        this.pc = pc;
        initPlayMode();
        load();
    }
    protected abstract void initPlayMode();

    public void dispose()
    {
        disposeImpl();
        save();
    }
    protected abstract void disposeImpl();

    public abstract void display();

    protected abstract void save();
    protected abstract void load();
}
