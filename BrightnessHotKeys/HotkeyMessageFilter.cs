namespace BrightnessHotKeys;

internal class HotkeyMessageFilter : IMessageFilter
{
    private readonly Action<int> hotkeyAction;

    public HotkeyMessageFilter(Action<int> hotkeyAction)
    {
        this.hotkeyAction = hotkeyAction ?? throw new ArgumentNullException(nameof(hotkeyAction));
    }

    public bool PreFilterMessage(ref Message m)
    {
        const int wmHotkey = 0x0312;

        if (m.Msg != wmHotkey) 
            return false;

        var id = m.WParam.ToInt32();
        hotkeyAction(id);
        return true;

    }
}