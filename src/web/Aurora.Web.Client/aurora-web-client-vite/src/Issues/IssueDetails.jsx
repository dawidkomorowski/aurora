import { useState, useEffect } from "react";
import { IssuesServiceClient } from "./IssuesServiceClient";
import { useParams } from "react-router-dom";


export function IssueDetails() {
    const { issueId } = useParams();
    const [data, setData] = useState([]);

    useEffect(() => {
        IssuesServiceClient.get(issueId).then(responseData => {
            setData(responseData);
        }).catch(error => {
            console.error(error);
        });
    }, []);

    return (
        <div style={{ display: "flex", justifyContent: "center" }}>
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }} >
                <div><strong>Id: </strong>{data.id}</div>
                <div><h1>{data.title}</h1></div>
                <div><h4>Description</h4></div>
                <div>{data.description}</div>
                <br />
                <div><strong>Status: </strong>{data.status}</div>
                <br />
                <div><strong>Created: </strong>{data.createdDateTime}</div>
                <div><strong>Updated: </strong>{data.updatedDateTime}</div>
            </div>
        </div >
    );
}
