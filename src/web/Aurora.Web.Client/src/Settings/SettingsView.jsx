import { useEffect, useState } from "react";
import { VersionItem } from "./VersionItem";
import { NewVersionComponent } from "./NewVersionComponent";
import { VersionApiClient } from "../ApiClients/VersionApiClient";

export function SettingsView() {
    const [versions, setVersions] = useState([]);

    useEffect(() => {
        refreshVersions();
    }, [])

    function refreshVersions() {
        VersionApiClient.getAll().then(responseData => {
            setVersions(responseData);
        }).catch(error => {
            console.error(error)
        });
    }

    function onCreateVersion(versionName) {
        VersionApiClient.create(versionName).then(() => {
            refreshVersions();
        }).catch(error => {
            console.error(error)
        });
    }

    const versionItems = versions.map(v => {
        return (
            <VersionItem key={v.id} id={v.id} name={v.name} onRefreshRequested={refreshVersions} />
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

