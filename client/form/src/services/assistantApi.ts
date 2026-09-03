export interface AiChatRequest {
  message: string;
}

export interface AiChatResponse {
  message: string;
}

export async function sendAssistantMessage(
  message: string
): Promise<AiChatResponse> {
  const response = await fetch(
    "http://localhost:5000/api/assistant/chat",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        message
      })
    }
  );

  if (!response.ok) {
    throw new Error("Assistant request failed");
  }

  return response.json();
}