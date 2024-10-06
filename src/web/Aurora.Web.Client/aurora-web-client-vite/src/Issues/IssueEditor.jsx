
export function IssueEditor({ data, setData }) {
    function handleTitleChange(event) {
    }

    function handleDescriptionChange(event) {
    }

    return (
        <div>
            <div><strong>Title</strong></div>
            <div style={{ paddingRight: "10px" }}>
                <input type="text" value={data.title} onChange={handleTitleChange} style={{ width: "100%" }} />
            </div>
            <br />
            <div><strong>Description</strong></div>
            <div style={{ paddingRight: "10px" }}>
                <textarea value={data.description} onChange={handleDescriptionChange} style={{ width: "100%", height: "500px", resize: "none" }} />
            </div>
        </div>
    );
}
