using System;
using System.Collections.Generic;
using System.Text;

namespace VisionProcess.ViewModel.ViewServices;

public interface ICultureSettingService
{
    void ChangeCulture(string language);

    void ChangeCulture(int lcid);

    int GetCurrentCultureLCID();
}
