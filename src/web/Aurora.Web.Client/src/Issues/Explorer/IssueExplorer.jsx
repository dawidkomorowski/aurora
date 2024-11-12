import { useEffect, useState } from "react";
import { IssueList } from "./IssueList";
import { IssueApiClient } from "../../ApiClients/IssueApiClient";
import { ShowAllVersionFilter, VersionFilter } from "./VersionFilter";

export function IssueExplorer() {
    const [statusFilter, setStatusFilter] = useState("");
    const [versionFilter, setVersionFilter] = useState(ShowAllVersionFilter);
    const [data, setData] = useState([]);

    useEffect(() => {
        const filters = {
            status: statusFilter || null
        };

        IssueApiClient.getAll(filters).then(responseData => {
            setData(responseData);
        }).catch(error => {
            console.error(error)
        });
    }, [statusFilter, versionFilter]);

    function handleStatusFilterInput(event) {
        setStatusFilter(event.target.value);
    }

    return (
        <div>
            <div style={{ display: "flex" }}>
                <div style={{ display: "flex" }}>
                    <div style={{ margin: "10px" }}>
                        <strong>Status</strong>
                    </div>
                    <select value={statusFilter} onInput={handleStatusFilterInput} style={{ width: "200px" }}>
                        <option value="">All</option>
                        <option value="Open">Open</option>
                        <option value="In Progress">In Progress</option>
                        <option value="Closed">Closed</option>
                    </select>
                </div>
                <div style={{ display: "flex" }}>
                    <div style={{ margin: "10px" }}>
                        <strong>Version</strong>
                    </div>
                    <VersionFilter versionFilter={versionFilter} setVersionFilter={setVersionFilter} />
                </div>
            </div>
            <IssueList data={data} />
        </div>
    );
}

