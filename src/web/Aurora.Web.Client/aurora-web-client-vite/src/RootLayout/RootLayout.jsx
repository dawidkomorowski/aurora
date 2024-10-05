import { Link, Outlet, useRouteError } from "react-router-dom";

export function RootLayout() {
    return (
        <>
            <div>
                <nav>
                    <ul>
                        <li>
                            <Link to="/issue-explorer">Issue Explorer</Link>
                        </li>
                    </ul>
                </nav>
            </div>
            <div>
                <Outlet />
            </div>
        </>
    );
}

