import { useParams } from "react-router-dom";
import { useState, useEffect } from "react";
import { IssuesServiceClient } from "./IssuesServiceClient";
import { IssueDetails } from "./IssueDetails";
import { IssueEditor } from "./IssueEditor";

export function IssueDetailsView() {
    const { issueId } = useParams();
    const [editMode, setEditMode] = useState(false);
    const [data, setData] = useState([]);

    useEffect(() => {
        IssuesServiceClient.get(issueId).then(responseData => {
            setData(responseData);
        }).catch(error => {
            console.error(error);
        });
    }, []);

    function handleEditButtonClick() {
        setEditMode(true);
    }

    function handleSaveButtonClick() {
        setEditMode(false);
    }

    let content;
    let button;
    if (editMode) {
        content = <div style={{ marginTop: "20px" }}><IssueEditor data={data} setData={setData} /></div>
        button = <button onClick={handleSaveButtonClick}>Save</button>
    }
    else {
        content = <IssueDetails title={data.title} description={data.description} status={data.status} />
        button = <button onClick={handleEditButtonClick}>Edit</button>
    }

    return (
        <div style={{ display: "flex", justifyContent: "center" }}>
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }} >
                <div><strong>Id: </strong>{data.id}</div>
                {content}
                <br />
                <div><strong>Created: </strong>{data.createdDateTime}</div>
                <div><strong>Updated: </strong>{data.updatedDateTime}</div>
                <div style={{ marginTop: "20px", float: "right" }}>
                    {button}
                </div>
            </div>
        </div >
    );
}
