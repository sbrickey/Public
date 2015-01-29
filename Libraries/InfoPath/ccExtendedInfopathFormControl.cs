using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBrickey.InfoPath.Controls
{
    public class ccExtendedInfopathFormControl : Microsoft.Office.InfoPath.FormControl
    {
        private Generics.ObservableWatchDispatcherIdle<bool?> ipForm_XmlForm_IsLoaded = null;

        public event EventHandler FormLoaded;
        private void FormLoaded_Fire(EventArgs e) { var handler = this.FormLoaded; if (handler != null) { handler(this, e); } }

        public event EventHandler FormUnloaded;
        private void FormUnloaded_Fire(EventArgs e) { var handler = this.FormUnloaded; if (handler != null) { handler(this, e); } }

        // CTor
        public ccExtendedInfopathFormControl() : base()
        {
            this.CreateControl();

            if (!this.DesignMode)
            {
                this.ipForm_XmlForm_IsLoaded = new Generics.ObservableWatchDispatcherIdle<bool?>(
                    GetValueDelegate: () =>
                    {
                        return this.XmlForm != null;
                    },
                    OnValueChangedDelegate: (s2, e2) =>
                    {
                        // null to null : nothing
                        if (!e2.OldValue.HasValue &&
                            !e2.NewValue.HasValue)
                        { return; }


                        // null to false : nothing
                        if (!e2.OldValue.HasValue &&
                            e2.NewValue.HasValue && !e2.NewValue.Value)
                        { return; }

                        // null to true : Loaded
                        if (!e2.OldValue.HasValue &&
                            e2.NewValue.HasValue && e2.NewValue.Value)
                        { this.FormLoaded_Fire(new EventArgs()); return; }


                        // false to true : loaded
                        if (e2.OldValue.HasValue && !e2.OldValue.Value &&
                            e2.NewValue.HasValue && e2.NewValue.Value)
                        { this.FormLoaded_Fire(new EventArgs()); return; }

                        // true to false : unloaded
                        if (e2.OldValue.HasValue && e2.OldValue.Value &&
                            e2.NewValue.HasValue && !e2.NewValue.Value)
                        { this.FormUnloaded_Fire(new EventArgs()); return; }


                        // false to null : nothing
                        if (e2.OldValue.HasValue && !e2.OldValue.Value &&
                            !e2.NewValue.HasValue)
                        { return; }

                        // true to null : unloaded
                        if (e2.OldValue.HasValue && e2.OldValue.Value &&
                            !e2.NewValue.HasValue)
                        { this.FormUnloaded_Fire(new EventArgs()); return; }
                    },
                    Delay: TimeSpan.FromSeconds(0.2),
                    AutoStart: true
                );
            } // if !DesignMode
        }

    } // class
} // namespace