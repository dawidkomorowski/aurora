import { useEffect, useState } from "react";
import { VersionApiClient } from "../../ApiClients/VersionApiClient";

export function VersionFilter({ versionFilter, setVersionFilter }) {
    const [versions, setVersions] = useState([]);

    useEffect(() => {
        VersionApiClient.getAll().then(responseData => {
            setVersions([ShowAllVersionFilter, ShowUnassignedVersionFilter, ...responseData]);
        }).catch(error => {
            console.error(error)
        });
    }, []);

    function handleInput(event) {
        const versionId = parseInt(event.target.value);
        const selectedVersion = versions.find(v => v.id === versionId);
        setVersionFilter(selectedVersion);
    }

    const options = versions.map(v => <option key={v.id} value={v.id}>{v.name}</option>);

    return (
        <select value={versionFilter.id} onInput={handleInput} style={{ width: "200px" }}>
            {options}
        </select>
    );
}

export const ShowAllVersionFilter = {
    id: -1,
    name: "Show all"
}

export const ShowUnassignedVersionFilter = {
    id: 0,
    name: "Show unassigned"
}