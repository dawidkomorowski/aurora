import { useEffect, useState } from "react";
import { VersionItem } from "./VersionItem";
import { NewVersionComponent } from "./NewVersionComponent";

export function SettingsView() {
    const [versions, setVersions] = useState([]);

    useEffect(() => {
        setVersions([
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
        ]);
    }, [])

    function onCreateVersion(versionName) {
        const newVersion = {
            id: Math.round(Math.random() * 1000000),
            name: versionName
        }
        setVersions([...versions, newVersion]);
    }

    const versionItems = versions.map(v => {
        return (
            <VersionItem key={v.id} id={v.id} name={v.name} />
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
                <NewVersionComponent onCreate={onCreateVersion} />
            </div>
        </div >
    );
}

