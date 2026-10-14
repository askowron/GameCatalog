using System.ComponentModel;
using GameCatalog.UI.Components;
using GameCatalog.UI.Icons;

namespace GameCatalog.UI.Tests;

public class ActionListTests
{
    [Fact]
    public void SetAction_CopiesPropertiesToMenuItemAndButton()
    {
        using var container = new Container();
        var list = new ActionList(container);
        var action = new UIAction(container) { Text = "&Save", Icon = AmcIcon.FileSave, ShortcutKeys = Keys.Control | Keys.S };
        var menu = new ToolStripMenuItem();
        var button = new ToolStripButton();

        list.SetAction(menu, action);
        list.SetAction(button, action);

        Assert.Equal("&Save", menu.Text);
        Assert.Equal(Keys.Control | Keys.S, menu.ShortcutKeys);
        Assert.Equal((int)AmcIcon.FileSave, menu.ImageIndex);
        Assert.Equal("Save", button.ToolTipText);
        Assert.Same(action, ActionList.FindAction(button));
    }

    [Fact]
    public void ActionChanges_PropagateToBoundItems()
    {
        using var container = new Container();
        var list = new ActionList(container);
        var action = new UIAction(container) { Text = "A" };
        var menu = new ToolStripMenuItem();
        list.SetAction(menu, action);

        action.Enabled = false;
        action.Checked = true;
        action.Text = "B";

        Assert.False(menu.Enabled);
        Assert.True(menu.Checked);
        Assert.Equal("B", menu.Text);
    }

    [Fact]
    public void ItemClick_ExecutesAction_OnlyWhenEnabled()
    {
        using var container = new Container();
        var list = new ActionList(container);
        var action = new UIAction(container);
        int executed = 0;
        action.Execute += (_, _) => executed++;
        var menu = new ToolStripMenuItem();
        list.SetAction(menu, action);

        menu.PerformClick();
        action.Enabled = false;
        action.PerformExecute();

        Assert.Equal(1, executed);
    }

    [Fact]
    public void UpdateActions_RaisesUpdateOncePerAction()
    {
        using var container = new Container();
        var list = new ActionList(container) { AutoUpdate = false };
        var action = new UIAction(container);
        int updates = 0;
        action.Update += (_, _) => updates++;
        list.SetAction(new ToolStripMenuItem(), action);
        list.SetAction(new ToolStripButton(), action);

        list.UpdateActions();

        Assert.Equal(1, updates);
    }

    [Fact]
    public void SetAction_Null_Unbinds()
    {
        using var container = new Container();
        var list = new ActionList(container);
        var action = new UIAction(container) { Text = "A" };
        var menu = new ToolStripMenuItem();
        list.SetAction(menu, action);
        list.SetAction(menu, null);
        action.Text = "B";
        Assert.Equal("A", menu.Text);
        Assert.Null(ActionList.FindAction(menu));
    }
}
