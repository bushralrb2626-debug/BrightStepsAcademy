namespace BrightStepsAcademy.Data;

/// <summary>
/// Central image catalog. Replace these URLs with uploaded media later.
/// </summary>
public static class Images
{
    private static string U(string id, int w = 900) =>
        $"https://images.unsplash.com/{id}?auto=format&fit=crop&w={w}&q=80";

    public static string Hero => U("photo-1503676260728-1c00da094a0b", 1200);
    public static string School => U("photo-1580582932707-520aed937b7b", 1100);
    public static string Campus => U("photo-1562774053-701939374585", 1000);
    public static string Classroom => U("photo-1588072432836-e10032774350", 900);
    public static string Library => U("photo-1524995997946-a1c2e315a42f", 1000);
    public static string Science => U("photo-1567427013422-6b76b26f2254", 900);
    public static string Computers => U("photo-1509062522246-3755977927d7", 900);
    public static string Sports => U("photo-1461896836934-ffe607ba6851", 1000);
    public static string Art => U("photo-1513364776144-60967b0f800f", 900);
    public static string Music => U("photo-1511379938547-c1f69419868d", 900);
    public static string Play => U("photo-1503454537195-1dcabb73ffb9", 900);
    public static string Cafeteria => U("photo-1567521464027-f127ff144326", 900);
    public static string Medical => U("photo-1576091160399-112ba8d25d1d", 900);
    public static string Bus => U("photo-1544620341-11cb2cd96ace", 900);
    public static string SmartClass => U("photo-1509062522246-3755977927d7", 900);
    public static string Security => U("photo-1557597774-9d273605dfa9", 900);
    public static string KidsRead => U("photo-1503676260728-1c00da094a0b", 800);
    public static string FieldTrip => U("photo-1476514525535-07fb3b4ae5f1", 900);
    public static string ScienceFair => U("photo-1532094349884-543bc11b234d", 900);
    public static string SportsDay => U("photo-1517649763962-0c623066013b", 900);
    public static string Annual => U("photo-1511578314322-379afb476865", 900);
    public static string Reading => U("photo-14565130808-af32baae6e9d", 900);
    public static string Picnic => U("photo-1464207687429-7505649dae38", 900);
    public static string BookFair => U("photo-1481627834876-b7833e8f5570", 900);
    public static string Meeting => U("photo-1577896851231-70ef18881754", 900);
    public static string Trophy => U("photo-1567427017947-545c5f8d16ad", 800);
    public static string Portal => U("photo-1427504494785-3a9ca7044f4a", 1000);
    public static string Login => U("photo-1588072432836-e10032774350", 1000);

    public static string[] Teachers =>
    [
        U("photo-1580894732444-8ecded7900cd", 400),
        U("photo-1573496359142-b8d87734a5a2", 400),
        U("photo-1500648767791-00dcc994a43e", 400),
        U("photo-1544005313-94ddf0286df2", 400),
        U("photo-1507003211169-0a1dd7228f2d", 400),
        U("photo-1438761681033-6461ffad8d80", 400),
        U("photo-1472099645785-5658abf4ff4e", 400),
        U("photo-1547425260-76bcadfb4f2c", 400)
    ];

    public static string[] Students =>
    [
        U("photo-1544717305-2782549b5136", 400),
        U("photo-1516627145497-ae6968895b74", 400),
        U("photo-1503919545889-aef636e10ad4", 400),
        U("photo-1595956553066-fe24a8c61195", 400),
        U("photo-1608889175123-8ee362201f81", 400),
        U("photo-1519457431-44ccd64a579b", 400),
        U("photo-1508214751196-bcfd4ca60f91", 400),
        U("photo-1546525848-3ce03ca516f6", 400),
        U("photo-1581833971358-2c8b550f87b3", 400),
        U("photo-1529626455594-4ff0802cfb7e", 400),
        U("photo-1544005313-94ddf0286df2", 400),
        U("photo-1524504388940-b1c1722653e1", 400)
    ];

    public static string[] Parents =>
    [
        U("photo-1544005313-94ddf0286df2", 400),
        U("photo-1500648767791-00dcc994a43e", 400),
        U("photo-1573496359142-b8d87734a5a2", 400),
        U("photo-1507003211169-0a1dd7228f2d", 400),
        U("photo-1438761681033-6461ffad8d80", 400),
        U("photo-1472099645785-5658abf4ff4e", 400)
    ];

    public static string[] Gallery =>
    [
        Classroom, KidsRead, Library, Sports, Art, Science,
        Campus, School, Play, Annual, Music, FieldTrip
    ];
}
