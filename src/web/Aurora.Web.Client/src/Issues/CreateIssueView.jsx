import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { IssueApiClient } from "../ApiClients/IssueApiClient";

// TODO Acceptance Criteria:
// TODO - In Create Issue view there is a Version field.
// TODO - In Create Issue view a Version field is empty by default meaning no version assigned.
// TODO - In Create Issue view a Version field can be set by choosing one of predefined options from a drop down. Values are the ones defined in settings.
// TODO - In Create Issue view a Version field can be set to empty by choosing appropriate option from a drop down.

export function CreateIssueView() {
    const navigate = useNavigate();
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");

    function handleTitleInput(event) {
        setTitle(event.target.value);
    }

    function handleDescriptionInput(event) {
        setDescription(event.target.value);
    }

    function handleCreateButtonClick() {
        IssueApiClient.create(title, description).then(responseData => {
            navigate(`/issue/${responseData.id}`);
        }).catch(error => {
            console.error(error);
        });
    }

    return (
        <div style={{ display: "flex", justifyContent: "center" }}>
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }}>
                <div><strong>Title</strong></div>
                <div style={{ paddingRight: "10px" }}>
                    <input type="text" value={title} onInput={handleTitleInput} style={{ width: "100%" }} />
                </div>
                <br />
                <div><strong>Description</strong></div>
                <div style={{ paddingRight: "10px" }}>
                    <textarea value={description} onInput={handleDescriptionInput} style={{ width: "100%", height: "500px", resize: "none" }} />
                </div>
                <div style={{ marginTop: "20px", float: "right" }}>
                    <button onClick={handleCreateButtonClick}>Create</button>
                </div>
            </div>
        </div>
    );
}
