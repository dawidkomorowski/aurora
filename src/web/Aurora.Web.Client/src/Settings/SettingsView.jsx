export function SettingsView() {
    const versions = [
        {
            id: 1,
            name: "Version 1"
        },
        {
            id: 2,
            name: "Version 2"
        },
        {
            id: 3,
            name: "Version 3"
        }
    ];

    var versionItems = versions.map(v => {
        return (
            <div>{v.name}</div>
        );
    });

    return (
        <div style={{ display: "flex", justifyContent: "center" }}>
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }} >
                <span>
                    <h1>Settings</h1>
                </span>
                <br />
                <span>
                    <h2>Versions</h2>
                </span>
                <div>
                    {versionItems}
                </div>
            </div>
        </div >
    );
}