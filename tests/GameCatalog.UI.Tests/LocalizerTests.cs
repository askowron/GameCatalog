using System.ComponentModel;
using GameCatalog.UI.Components;

namespace GameCatalog.UI.Tests;

public class LocalizerTests
{
    private sealed class SampleForm : Form
    {
        public readonly IContainer Parts = new Container();
        public readonly Label lblHello = new() { Name = "lblHello", Text = "Cześć" };
        public readonly Button btnOk = new() { Name = "btnOk", Text = "OK" };
        public readonly TextBox txtData = new() { Name = "txtData", Text = "user data" };
        public readonly MenuStrip menu = new() { Name = "menu" };
        public readonly ToolStripMenuItem mnuFile = new() { Name = "mnuFile", Text = "&Plik" };
        public readonly ToolStripMenuItem mnuFileSave = new() { Name = "mnuFileSave", Text = "zapisz-design" };
        public readonly ListView list = new() { Name = "list" };
        public readonly ColumnHeader colTitle = new() { Name = "colTitle", Text = "Tytuł" };
        public readonly UIAction actSave;
        public readonly ActionList actions;
        public readonly Localizer localizer;

        public SampleForm()
        {
            Text = "Formularz";
            mnuFile.DropDownItems.Add(mnuFileSave);
            menu.Items.Add(mnuFile);
            list.Columns.Add(colTitle);
            Controls.AddRange(new Control[] { lblHello, btnOk, txtData, menu, list });
            actSave = new UIAction(Parts) { Key = "FileSave", Text = "Zapisz" };
            actions = new ActionList(Parts) { AutoUpdate = false };
            actions.SetAction(mnuFileSave, actSave);
            localizer = new Localizer(Parts) { ContainerControl = this };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Parts.Dispose();
            base.Dispose(disposing);
        }
    }

    private static FakeTranslator English()
    {
        var t = new FakeTranslator();
        t.Forms[("SampleForm", "$this.Text")] = "Form";
        t.Forms[("SampleForm", "lblHello.Text")] = "Hello";
        t.Forms[("SampleForm", "mnuFile.Text")] = "&File";
        t.Forms[("SampleForm", "colTitle.Text")] = "Title";
        t.Forms[("SampleForm", "mnuFileSave.Text")] = "MUST NOT BE USED";
        t.Strings["Action.FileSave"] = "Save";
        t.Strings["Action.FileSave.Hint"] = "Save the catalog";
        return t;
    }

    [Fact]
    public void Apply_TranslatesCaptionsAndActions_KeepsUserData()
    {
        Localizer.Translator = English();
        using var f = new SampleForm();
        f.localizer.Apply();

        Assert.Equal("Form", f.Text);
        Assert.Equal("Hello", f.lblHello.Text);
        Assert.Equal("OK", f.btnOk.Text);                 // no translation → designer text
        Assert.Equal("user data", f.txtData.Text);         // editors are never touched
        Assert.Equal("&File", f.mnuFile.Text);
        Assert.Equal("Title", f.colTitle.Text);
        Assert.Equal("Save", f.mnuFileSave.Text);          // bound item follows its action
        Assert.Equal("Save the catalog", f.actSave.ToolTipText);
    }

    [Fact]
    public void LanguageSwitch_RestoresDesignerTextsWhenKeyMissing()
    {
        var t = English();
        Localizer.Translator = t;
        using var f = new SampleForm();
        f.localizer.Apply();

        t.Forms.Clear();
        t.Strings.Clear();
        t.Switch("pl");

        Assert.Equal("Cześć", f.lblHello.Text);
        Assert.Equal("Formularz", f.Text);
        Assert.Equal("Zapisz", f.mnuFileSave.Text);
    }

    [Fact]
    public void CollectKeys_ListsTranslatableProperties()
    {
        Localizer.Translator = new FakeTranslator();
        using var f = new SampleForm();
        var keys = f.localizer.CollectKeys().Select(k => k.Key).ToList();

        Assert.Contains("$this.Text", keys);
        Assert.Contains("lblHello.Text", keys);
        Assert.Contains("btnOk.Text", keys);
        Assert.Contains("mnuFile.Text", keys);
        Assert.Contains("colTitle.Text", keys);
        Assert.DoesNotContain("txtData.Text", keys);
        Assert.DoesNotContain("mnuFileSave.Text", keys);
        Assert.All(f.localizer.CollectKeys(), k => Assert.Equal("SampleForm", k.Form));
    }

    [Fact]
    public void LanguageSwitch_AfterFormDisposed_DoesNotApplyOrThrow()
    {
        var t = English();
        Localizer.Translator = t;
        var f = new SampleForm();
        f.localizer.Apply();
        var label = f.lblHello;
        f.Dispose();
        Assert.Equal(0, t.SubscriberCount);

        t.Forms.Clear();
        t.Switch("pl");

        Assert.Equal("Hello", label.Text);
    }

    [Fact]
    public void ParameterlessLocalizer_UnsubscribesWhenContainerDisposed()
    {
        var t = English();
        Localizer.Translator = t;
        using var form = new Form { Text = "x" };
        using var localizer = new Localizer { ContainerControl = form };
        Assert.Equal(1, t.SubscriberCount);

        form.Dispose();
        Assert.Equal(0, t.SubscriberCount);
        t.Switch("en");
    }

    [Fact]
    public void Dispose_UnsubscribesFromOriginalTranslator_EvenAfterStaticChanged()
    {
        var first = English();
        Localizer.Translator = first;
        var f = new SampleForm();
        Localizer.Translator = English();
        f.Dispose();
        Assert.Equal(0, first.SubscriberCount);
    }
}
