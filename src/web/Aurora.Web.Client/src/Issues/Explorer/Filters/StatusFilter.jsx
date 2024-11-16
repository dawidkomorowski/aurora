import { useState, useEffect } from "react";
import { useSearchFilters } from "./useSearchFilters";

export function StatusFilter() {
    const [searchFilters, setSearchFilters] = useSearchFilters();
    const [status, setStatus] = useState(searchFilters.status ?? "All");

    useEffect(() => {
        const newSearchFilters = {
            ...searchFilters,
            status: status === "All" ? null : status
        };
        setSearchFilters(newSearchFilters);
    }, [status]);

    useEffect(() => {
        setStatus(searchFilters.status ?? "All");
    }, [searchFilters]);

    function handleInput(event) {
        setStatus(event.target.value);
    }

    return (
        <select value={status} onInput={handleInput} style={{ width: "200px" }}>
            <option value="All">All</option>
            <option value="Open">Open</option>
            <option value="In Progress">In Progress</option>
            <option value="Closed">Closed</option>
        </select>
    );
}
