import type { AiAction } from "../../services/assistantApi";
import type { AiActionHandler } from "./types";

export function createNavigationHandler(
    navigate: (path: string) => void
): AiActionHandler {
    return (action: AiAction) => {
        const path = action.data.path;
    
        if (typeof path !== "string" || !path) {
            throw new Error(`Invalid navigation path '${path}'`);
        }

        navigate(path);
    };
}