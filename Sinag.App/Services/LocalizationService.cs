using System.ComponentModel;

namespace Sinag.App.Services;

public class LocalizationService : INotifyPropertyChanged
{
    private static LocalizationService? _instance;
    public static LocalizationService Instance => _instance ??= new();

    private string _currentLanguage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public LocalizationService()
    {
        _currentLanguage = Preferences.Get("app_language", "fil");
    }

    public string this[string key] =>
        Strings.TryGetValue(_currentLanguage, out var dict) && dict.TryGetValue(key, out var value)
            ? value
            : key;

    public string CurrentLanguage => _currentLanguage;

    public void SetLanguage(string code)
    {
        if (_currentLanguage == code) return;
        _currentLanguage = code;
        Preferences.Set("app_language", code);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    // ── All translations ────────────────────────────────────────────

    private static readonly Dictionary<string, Dictionary<string, string>> Strings = new()
    {
        ["fil"] = new()
        {
            // Common
            ["AppName"] = "Sinag",
            ["Copyright"] = "© 2025 Sinag Solar. Para sa Davao City.",
            ["PoweredBy"] = "POWERED BY SINAG DAVAO",
            ["KwhUnit"] = "kWh",
            ["PesoKwhUnit"] = "₱/kWh",
            ["DaysUnit"] = "araw",
            ["IncludeBattery"] = "Isama ang Battery?",

            // Home
            ["Home_Badge"] = "DAVAO CITY ENERGY",
            ["Home_Headline"] = "Solar Estimate para sa Davao",
            ["Home_Subtitle"] = "Gawing katuwang ang araw. Tuklasin kung gaano kalaki ang matitipid mo sa kuryente gamit ang aming matalinong solar calculator.",
            ["Home_ScanButton"] = "I-scan ang Iyong Bill",
            ["Home_ManualLink"] = "O mag-type ng manual →",
            ["Home_HowItWorks"] = "Paano Ito Gumagana",
            ["Home_Step1Title"] = "I-scan",
            ["Home_Step1Desc"] = "Kunan ng litrato ang iyong electric bill. Babasahin ng aming AI ang iyong konsumo nang mabilisan.",
            ["Home_Step2Title"] = "Suriin",
            ["Home_Step2Desc"] = "Tingnan at i-verify ang mga nakuha naming detalye para masiguradong tama ang bawat numero.",
            ["Home_Step3Title"] = "Resulta",
            ["Home_Step3Desc"] = "Makakuha ng detalyadong solar estimate, kasama ang laki ng system at kung kailan ito mababawi.",
            ["Home_BannerTitle"] = "Handa na para sa Davao Sunshine?",
            ["Home_BannerDesc"] = "Samahan ang libo-libong pamilya sa Davao na lumipat na sa mas mura at malinis na enerhiya.",

            // Scan
            ["Scan_Title"] = "I-scan ang Bill",
            ["Scan_Guide"] = "GABAY SA PAG-SCAN",
            ["Scan_DlpcBill"] = "DLPC Bill",
            ["Scan_TakePhoto"] = "Kunan ng Litrato",
            ["Scan_PickGallery"] = "Pumili mula sa Gallery",
            ["Scan_ManualLink"] = "I-type ang Manual →",
            ["Scan_Ready"] = "Nakahanda na ang iyong Sinag reader...",

            // Review
            ["Review_Title"] = "Suriin ang Iyong Bill Data",
            ["Review_ScanComplete"] = "AI SCAN COMPLETE",
            ["Review_VerifyPrefix"] = "Pakisuri kung tama ang ",
            ["Review_VerifyHighlight"] = "impormasyon",
            ["Review_KwhConsumed"] = "KWH CONSUMED",
            ["Review_KwhPlaceholder"] = "Hal. 350",
            ["Review_BillingPeriod"] = "BILLING PERIOD",
            ["Review_BillingMonth"] = "BILLING MONTH",
            ["Review_GenCharge"] = "GENERATION CHARGE",
            ["Review_TotalAmount"] = "TOTAL AMOUNT DUE",
            ["Review_TotalPlaceholder"] = "Hal. 3,500.00",
            ["Review_Confirm"] = "Mukhang Tama",
            ["Review_Rescan"] = "I-scan Muli",

            // Manual Entry
            ["Manual_Title"] = "I-type ang Iyong Bill Data",
            ["Manual_Headline"] = "Detalye ng Kuryente",
            ["Manual_Subtitle"] = "Pakilagay ang mga impormasyon mula sa iyong huling bill upang makuha ang pinaka-accurate na kalkulasyon.",
            ["Manual_UsageSection"] = "Paggamit (Usage)",
            ["Manual_KwhPlaceholder"] = "Halimbawa: 250",
            ["Manual_DaysLabel"] = "BILANG NG ARAW",
            ["Manual_MonthLabel"] = "BUWAN NG BILL",
            ["Manual_ChargesSection"] = "Halaga at Charges",
            ["Manual_GenChargeLabel"] = "GENERATION CHARGE (PER KWH)",
            ["Manual_TotalAmount"] = "TOTAL AMOUNT DUE",
            ["Manual_TotalHint"] = "Tingnan ang 'Current Amount' sa iyong bill.",
            ["Manual_BatteryDesc"] = "Mag-store ng enerhiya para sa gabi.",
            ["Manual_Calculate"] = "Kalkulahin",

            // Results
            ["Results_CalcResult"] = "RESULTA NG KALKULASYON",
            ["Results_RecommendedFormat"] = "Inirerekomendang Sistema: {0:F1} kWp",
            ["Results_SunHoursDesc"] = "Mayroong 4.8 peak sun hours sa Davao City. Ang iyong bubong ay may sapat na sikat ng araw para sa iyong pangangailangan.",
            ["Results_YourBenefits"] = "IYONG BENEPISYO",
            ["Results_MonthlySavingsFormat"] = "Buwanang Savings: ₱{0:N0}",
            ["Results_PaybackPeriod"] = "Payback Period",
            ["Results_PaybackFormat"] = "{0} buwan",
            ["Results_25YearSavings"] = "25-Year Savings",
            ["Results_BasedOnGenFormat"] = "Batay sa generation charge (₱{0:F2}/kWh)",
            ["Results_SystemDetails"] = "Detalye ng Sistema",
            ["Results_ChoosePackage"] = "Piliin ang package na angkop sa iyong budget.",
            ["Results_Budget"] = "Budget",
            ["Results_MidRange"] = "Mid-Range",
            ["Results_Premium"] = "Premium",
            ["Results_BOM"] = "Bill of Materials (BOM)",
            ["Results_TotalEstimate"] = "TOTAL ESTIMATE",
            ["Results_SolarPanels"] = "Solar Panels",
            ["Results_Inverter"] = "Inverter",
            ["Results_BatteryStorage"] = "Battery Storage",
            ["Results_Mounting"] = "Mounting & Structure",
            ["Results_WiringLabor"] = "Wiring & Labor",
            ["Results_Labor"] = "Labor",
            ["Results_Disclaimer"] = "Ang presyong ito ay estimasyon lamang base sa average market rates sa Davao City.",
            ["Results_Share"] = "I-share",
            ["Results_NextSteps"] = "Susunod na Hakbang",

            // Next Steps
            ["Next_Title"] = "Ano ang Susunod?",
            ["Next_Step1Title"] = "I-review kasama ang pamilya.",
            ["Next_Step1Desc"] = "Ibahagi ang iyong mga natuklasan sa calculator. Siguraduhin na ang buong pamilya ay handa para sa pagbabago sa inyong enerhiya.",
            ["Next_Step2Title"] = "Kontakin ang installer para sa survey.",
            ["Next_Step2Desc"] = "Mag-schedule ng site visit para masuri ang inyong bubong.",
            ["Next_Step3Title"] = "I-compare ang mga quotation.",
            ["Next_Step3Desc"] = "Suriin ang warranty, presyo, at serbisyo ng bawat company.",
            ["Next_InstallerDir"] = "Mga Solar Installer sa Davao",
            ["Next_InstallerSubtitle"] = "Mga pinagkakatiwalaang eksperto para sa iyong tahanan.",
            ["Next_Call"] = "📞 Tawag",
            ["Next_Website"] = "🌐 Website",
            ["Next_Disclaimer"] = "Ang Sinag ay hindi kaakibat ng alinmang installer. Gawin ang iyong sariling pananaliksik.",
            ["Next_GoHome"] = "Bumalik sa Home",

            // Language picker
            ["Lang_Filipino"] = "Filipino",
            ["Lang_English"] = "English",
            ["Lang_Bisaya"] = "Bisaya",
        },

        ["en"] = new()
        {
            // Common
            ["AppName"] = "Sinag",
            ["Copyright"] = "© 2025 Sinag Solar. Crafted for Davao City.",
            ["PoweredBy"] = "POWERED BY SINAG DAVAO",
            ["KwhUnit"] = "kWh",
            ["PesoKwhUnit"] = "₱/kWh",
            ["DaysUnit"] = "days",
            ["IncludeBattery"] = "Include Battery?",

            // Home
            ["Home_Badge"] = "DAVAO CITY ENERGY",
            ["Home_Headline"] = "Solar Estimate for Davao",
            ["Home_Subtitle"] = "Make the sun your partner. Discover how much you can save on electricity with our smart solar calculator.",
            ["Home_ScanButton"] = "Scan Your Bill",
            ["Home_ManualLink"] = "Or type manually →",
            ["Home_HowItWorks"] = "How It Works",
            ["Home_Step1Title"] = "Scan",
            ["Home_Step1Desc"] = "Take a photo of your electric bill. Our AI will read your consumption instantly.",
            ["Home_Step2Title"] = "Review",
            ["Home_Step2Desc"] = "Check and verify the details we captured to make sure every number is correct.",
            ["Home_Step3Title"] = "Results",
            ["Home_Step3Desc"] = "Get a detailed solar estimate, including system size and payback period.",
            ["Home_BannerTitle"] = "Ready for Davao Sunshine?",
            ["Home_BannerDesc"] = "Join thousands of Davao families who have switched to cheaper, cleaner energy.",

            // Scan
            ["Scan_Title"] = "Scan Bill",
            ["Scan_Guide"] = "SCAN GUIDE",
            ["Scan_DlpcBill"] = "DLPC Bill",
            ["Scan_TakePhoto"] = "Take Photo",
            ["Scan_PickGallery"] = "Choose from Gallery",
            ["Scan_ManualLink"] = "Type Manually →",
            ["Scan_Ready"] = "Your Sinag reader is ready...",

            // Review
            ["Review_Title"] = "Review Your Bill Data",
            ["Review_ScanComplete"] = "AI SCAN COMPLETE",
            ["Review_VerifyPrefix"] = "Please verify the ",
            ["Review_VerifyHighlight"] = "information",
            ["Review_KwhConsumed"] = "KWH CONSUMED",
            ["Review_KwhPlaceholder"] = "e.g. 350",
            ["Review_BillingPeriod"] = "BILLING PERIOD",
            ["Review_BillingMonth"] = "BILLING MONTH",
            ["Review_GenCharge"] = "GENERATION CHARGE",
            ["Review_TotalAmount"] = "TOTAL AMOUNT DUE",
            ["Review_TotalPlaceholder"] = "e.g. 3,500.00",
            ["Review_Confirm"] = "Looks Correct",
            ["Review_Rescan"] = "Scan Again",

            // Manual Entry
            ["Manual_Title"] = "Enter Your Bill Data",
            ["Manual_Headline"] = "Electricity Details",
            ["Manual_Subtitle"] = "Enter the information from your latest bill for the most accurate calculation.",
            ["Manual_UsageSection"] = "Usage",
            ["Manual_KwhPlaceholder"] = "e.g. 250",
            ["Manual_DaysLabel"] = "NUMBER OF DAYS",
            ["Manual_MonthLabel"] = "BILLING MONTH",
            ["Manual_ChargesSection"] = "Costs & Charges",
            ["Manual_GenChargeLabel"] = "GENERATION CHARGE (PER KWH)",
            ["Manual_TotalAmount"] = "TOTAL AMOUNT DUE",
            ["Manual_TotalHint"] = "Check the 'Current Amount' on your bill.",
            ["Manual_BatteryDesc"] = "Store energy for nighttime use.",
            ["Manual_Calculate"] = "Calculate",

            // Results
            ["Results_CalcResult"] = "CALCULATION RESULTS",
            ["Results_RecommendedFormat"] = "Recommended System: {0:F1} kWp",
            ["Results_SunHoursDesc"] = "Davao City has 4.8 peak sun hours. Your roof gets enough sunlight for your energy needs.",
            ["Results_YourBenefits"] = "YOUR BENEFITS",
            ["Results_MonthlySavingsFormat"] = "Monthly Savings: ₱{0:N0}",
            ["Results_PaybackPeriod"] = "Payback Period",
            ["Results_PaybackFormat"] = "{0} months",
            ["Results_25YearSavings"] = "25-Year Savings",
            ["Results_BasedOnGenFormat"] = "Based on generation charge (₱{0:F2}/kWh)",
            ["Results_SystemDetails"] = "System Details",
            ["Results_ChoosePackage"] = "Choose the package that fits your budget.",
            ["Results_Budget"] = "Budget",
            ["Results_MidRange"] = "Mid-Range",
            ["Results_Premium"] = "Premium",
            ["Results_BOM"] = "Bill of Materials (BOM)",
            ["Results_TotalEstimate"] = "TOTAL ESTIMATE",
            ["Results_SolarPanels"] = "Solar Panels",
            ["Results_Inverter"] = "Inverter",
            ["Results_BatteryStorage"] = "Battery Storage",
            ["Results_Mounting"] = "Mounting & Structure",
            ["Results_WiringLabor"] = "Wiring & Labor",
            ["Results_Labor"] = "Labor",
            ["Results_Disclaimer"] = "These prices are estimates only based on average market rates in Davao City.",
            ["Results_Share"] = "Share",
            ["Results_NextSteps"] = "Next Steps",

            // Next Steps
            ["Next_Title"] = "What's Next?",
            ["Next_Step1Title"] = "Review with family.",
            ["Next_Step1Desc"] = "Share your calculator findings. Make sure the whole family is ready for the energy switch.",
            ["Next_Step2Title"] = "Contact installer for survey.",
            ["Next_Step2Desc"] = "Schedule a site visit to assess your roof.",
            ["Next_Step3Title"] = "Compare quotations.",
            ["Next_Step3Desc"] = "Review the warranty, price, and service of each company.",
            ["Next_InstallerDir"] = "Solar Installers in Davao",
            ["Next_InstallerSubtitle"] = "Trusted experts for your home.",
            ["Next_Call"] = "📞 Call",
            ["Next_Website"] = "🌐 Website",
            ["Next_Disclaimer"] = "Sinag is not affiliated with any installer. Do your own research.",
            ["Next_GoHome"] = "Back to Home",

            // Language picker
            ["Lang_Filipino"] = "Filipino",
            ["Lang_English"] = "English",
            ["Lang_Bisaya"] = "Bisaya",
        },

        ["ceb"] = new()
        {
            // Common
            ["AppName"] = "Sinag",
            ["Copyright"] = "© 2025 Sinag Solar. Para sa Davao City.",
            ["PoweredBy"] = "POWERED BY SINAG DAVAO",
            ["KwhUnit"] = "kWh",
            ["PesoKwhUnit"] = "₱/kWh",
            ["DaysUnit"] = "adlaw",
            ["IncludeBattery"] = "Iapil ang Battery?",

            // Home
            ["Home_Badge"] = "DAVAO CITY ENERGY",
            ["Home_Headline"] = "Solar Estimate para sa Davao",
            ["Home_Subtitle"] = "Himoa ang adlaw nga imong kauban. Susiha kung pila ang matipigan nimo sa kuryente gamit ang among solar calculator.",
            ["Home_ScanButton"] = "I-scan ang Imong Bill",
            ["Home_ManualLink"] = "O mag-type og manual →",
            ["Home_HowItWorks"] = "Giunsa Kini Molihok",
            ["Home_Step1Title"] = "I-scan",
            ["Home_Step1Desc"] = "Kuhai og litrato ang imong electric bill. Basahon sa among AI ang imong konsumo dayon.",
            ["Home_Step2Title"] = "Susiha",
            ["Home_Step2Desc"] = "Tan-awa ug i-verify ang mga detalye para masiguro nga sakto ang tanan nga numero.",
            ["Home_Step3Title"] = "Resulta",
            ["Home_Step3Desc"] = "Makakuha ka og detalyadong solar estimate, apil ang gidak-on sa system ug payback period.",
            ["Home_BannerTitle"] = "Andam na para sa Davao Sunshine?",
            ["Home_BannerDesc"] = "Uban sa linibo ka pamilya sa Davao nga nibalhin na sa mas barato ug limpyo nga enerhiya.",

            // Scan
            ["Scan_Title"] = "I-scan ang Bill",
            ["Scan_Guide"] = "GIYA SA PAG-SCAN",
            ["Scan_DlpcBill"] = "DLPC Bill",
            ["Scan_TakePhoto"] = "Kuhaan og Litrato",
            ["Scan_PickGallery"] = "Pagpili gikan sa Gallery",
            ["Scan_ManualLink"] = "I-type og Manual →",
            ["Scan_Ready"] = "Andam na ang imong Sinag reader...",

            // Review
            ["Review_Title"] = "Susiha ang Imong Bill Data",
            ["Review_ScanComplete"] = "AI SCAN NAHUMAN",
            ["Review_VerifyPrefix"] = "Palihog susiha kung sakto ang ",
            ["Review_VerifyHighlight"] = "impormasyon",
            ["Review_KwhConsumed"] = "KWH NAGAMIT",
            ["Review_KwhPlaceholder"] = "Pananglitan: 350",
            ["Review_BillingPeriod"] = "BILLING PERIOD",
            ["Review_BillingMonth"] = "BUWAN SA BILL",
            ["Review_GenCharge"] = "GENERATION CHARGE",
            ["Review_TotalAmount"] = "TOTAL NGA BAYRUNON",
            ["Review_TotalPlaceholder"] = "Pananglitan: 3,500.00",
            ["Review_Confirm"] = "Murag Sakto",
            ["Review_Rescan"] = "I-scan Pag-usab",

            // Manual Entry
            ["Manual_Title"] = "I-type ang Imong Bill Data",
            ["Manual_Headline"] = "Detalye sa Kuryente",
            ["Manual_Subtitle"] = "Palihog ibutang ang impormasyon gikan sa imong bill para sa pinakasakto nga kalkulasyon.",
            ["Manual_UsageSection"] = "Paggamit (Usage)",
            ["Manual_KwhPlaceholder"] = "Pananglitan: 250",
            ["Manual_DaysLabel"] = "GIDAGHANON SA ADLAW",
            ["Manual_MonthLabel"] = "BUWAN SA BILL",
            ["Manual_ChargesSection"] = "Kantidad ug Charges",
            ["Manual_GenChargeLabel"] = "GENERATION CHARGE (PER KWH)",
            ["Manual_TotalAmount"] = "TOTAL NGA BAYRUNON",
            ["Manual_TotalHint"] = "Tan-awa ang 'Current Amount' sa imong bill.",
            ["Manual_BatteryDesc"] = "Mag-tipig og enerhiya para sa gabii.",
            ["Manual_Calculate"] = "Kalkulaha",

            // Results
            ["Results_CalcResult"] = "RESULTA SA KALKULASYON",
            ["Results_RecommendedFormat"] = "Girekomenda nga Sistema: {0:F1} kWp",
            ["Results_SunHoursDesc"] = "Ang Davao City adunay 4.8 peak sun hours. Ang imong atop adunay igo nga sikat sa adlaw para sa imong panginahanglan.",
            ["Results_YourBenefits"] = "IMONG BENEPISYO",
            ["Results_MonthlySavingsFormat"] = "Binulan nga Savings: ₱{0:N0}",
            ["Results_PaybackPeriod"] = "Payback Period",
            ["Results_PaybackFormat"] = "{0} bulan",
            ["Results_25YearSavings"] = "25-Year Savings",
            ["Results_BasedOnGenFormat"] = "Base sa generation charge (₱{0:F2}/kWh)",
            ["Results_SystemDetails"] = "Detalye sa Sistema",
            ["Results_ChoosePackage"] = "Pilia ang package nga angay sa imong budget.",
            ["Results_Budget"] = "Budget",
            ["Results_MidRange"] = "Mid-Range",
            ["Results_Premium"] = "Premium",
            ["Results_BOM"] = "Bill of Materials (BOM)",
            ["Results_TotalEstimate"] = "TOTAL ESTIMATE",
            ["Results_SolarPanels"] = "Solar Panels",
            ["Results_Inverter"] = "Inverter",
            ["Results_BatteryStorage"] = "Battery Storage",
            ["Results_Mounting"] = "Mounting & Structure",
            ["Results_WiringLabor"] = "Wiring & Labor",
            ["Results_Labor"] = "Labor",
            ["Results_Disclaimer"] = "Kini nga presyo estimasyon lamang base sa average market rates sa Davao City.",
            ["Results_Share"] = "I-share",
            ["Results_NextSteps"] = "Sunod nga Lakang",

            // Next Steps
            ["Next_Title"] = "Unsa ang Sunod?",
            ["Next_Step1Title"] = "I-review kauban ang pamilya.",
            ["Next_Step1Desc"] = "Ipaambit ang imong nakit-an sa calculator. Siguroha nga ang tibuok pamilya andam sa pagbalhin sa enerhiya.",
            ["Next_Step2Title"] = "Kontaka ang installer para sa survey.",
            ["Next_Step2Desc"] = "Mag-schedule og site visit para masusi ang inyong atop.",
            ["Next_Step3Title"] = "I-compare ang mga quotation.",
            ["Next_Step3Desc"] = "Susiha ang warranty, presyo, ug serbisyo sa matag company.",
            ["Next_InstallerDir"] = "Mga Solar Installer sa Davao",
            ["Next_InstallerSubtitle"] = "Mga kasaligan nga eksperto para sa imong panimalay.",
            ["Next_Call"] = "📞 Tawag",
            ["Next_Website"] = "🌐 Website",
            ["Next_Disclaimer"] = "Ang Sinag wala'y kalabutan sa bisan unsang installer. Buhata ang imong kaugalingong pag-research.",
            ["Next_GoHome"] = "Balik sa Home",

            // Language picker
            ["Lang_Filipino"] = "Filipino",
            ["Lang_English"] = "English",
            ["Lang_Bisaya"] = "Bisaya",
        }
    };
}
