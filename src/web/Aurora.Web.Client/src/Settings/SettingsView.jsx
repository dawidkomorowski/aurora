import { useEffect, useState } from "react";
import { VersionItem } from "./VersionItem";

export function SettingsView() {
    const [versions, setVersions] = useState([]);
    const [newVersionName, setNewVersionName] = useState("");

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

    function handleNewVersionInput(event) {
        setNewVersionName(event.target.value);
    }

    function handleCreateButtonClick() {
        const newVersion = {
            id: Math.round(Math.random() * 1000000),
            name: newVersionName
        }
        setVersions([...versions, newVersion]);
        setNewVersionName("");
    }

    var versionItems = versions.map(v => {
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
                <div style={{ marginTop: "5px" }}>
                    <input type="text" value={newVersionName} onInput={handleNewVersionInput} style={{ marginRight: "5px" }}></input>
                    <button onClick={handleCreateButtonClick}>Create</button>
                </div>
            </div>
        </div >
    );
}

