import { useEffect, useState } from "react";
import { IssuesList } from "./IssuesList";

export function IssuesExplorer() {
    const [data, setData] = useState([]);

    useEffect(() => {
        console.log("Fetch data");

        fetch("http://localhost:5180/api/issues").then(response => {
            if (!response.ok) {
                throw new Error(`Response status: ${response.status}`);
            }

            return response.json();
        }).then(responseData => {
            setData(responseData);
        }).catch(error => {
            console.error(error)
        });
    }, []);

    return (
        <>
            <IssuesList data={data} />
        </>
    );
}
