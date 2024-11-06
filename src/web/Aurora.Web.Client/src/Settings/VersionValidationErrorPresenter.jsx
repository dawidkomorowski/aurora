export function VersionValidationErrorPresenter({ validationErrors }) {
    const validationErrorItems = validationErrors.map(ve => <div key={ve} style={{ color: "red" }}>{ve}</div>);

    return (
        <div>
            {validationErrorItems}
        </div>
    );
}
