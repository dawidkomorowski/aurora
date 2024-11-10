import { useEffect, useState } from "react";
import { VersionApiClient } from "../ApiClients/VersionApiClient";

export function VersionSelect({ version, onVersionSelected }) {
    const [versions, setVersions] = useState([]);

    useEffect(() => {
        VersionApiClient.getAll().then(responseData => {
            setVersions([NoVersion, ...responseData]);
        }).catch(error => {
            console.error(error)
        });
    }, []);

    function handleVersionInput(event) {
        const versionId = parseInt(event.target.value);
        const selectedVersion = versions.find(v => v.id === versionId);
        onVersionSelected(selectedVersion);
    }

    const options = versions.map(v => <option key={v.id} value={v.id}>{v.name}</option>);

    return (
        <select value={version.id} onInput={handleVersionInput} style={{ width: "200px" }}>
            {options}
        </select>
    );
}

export const NoVersion = {
    id: 0,
    name: ""
}