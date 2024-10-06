import { useState } from "react";

export function CreateIssueView() {
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");

    return (
        <div style={{ display: "flex", justifyContent: "center" }}>
            <div style={{ width: "50%", backgroundColor: "lightgray", padding: "10px" }}>
                <div><strong>Title</strong></div>
                <div>{title}</div>
                <div><strong>Description</strong></div>
                <div>{description}</div>
            </div>
        </div>
    );
}
