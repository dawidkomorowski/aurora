import { useEffect, useState } from "react";
import { IssueList } from "./IssueList";
import { IssueApiClient } from "../../ApiClients/IssueApiClient";
import { ShowAllVersionFilter, VersionFilter } from "./VersionFilter";
import { useSearchParams } from "react-router-dom";

// TODO Refactor VersionFilter in a way that externally it operates only on a value that can be persisted in URL i.e. version ID.
// TODO When value from search params is not available in filter it should be set to some default value i.e. nonexistent version ID 123 is found in params but such version ID is not available in filters.
// TODO Refactor search params handling so it avoids duplication and is easier to extend and maintain. Consider using custom hook?

export function IssueExplorer() {
    const [searchParams, setSearchParams] = useSearchParams();
    const [statusFilter, setStatusFilter] = useState(searchParams.get("status") ?? "");
    const [versionFilter, setVersionFilter] = useState(searchParams.get("versionId") != null ? { id: searchParams.get("versionId") } : ShowAllVersionFilter);
    const [data, setData] = useState([]);

    useEffect(() => {
        const newSearchParams = new URLSearchParams(searchParams.toString());

        if (statusFilter) {
            newSearchParams.set("status", statusFilter);
        }
        else {
            newSearchParams.delete("status");
        }

        if (versionFilter === ShowAllVersionFilter) {
            newSearchParams.delete("versionId");
        }
        else {
            newSearchParams.set("versionId", versionFilter.id);
        }

        setSearchParams(newSearchParams);
    }, [statusFilter, versionFilter]);

    useEffect(() => {
        const filters = {
            status: statusFilter || null,
            versionId: null
        };

        if (versionFilter !== ShowAllVersionFilter) {
            filters.versionId = versionFilter.id;
        }

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

