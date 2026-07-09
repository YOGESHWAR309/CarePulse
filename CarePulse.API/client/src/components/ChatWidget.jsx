import { useState, useRef, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { apiFetch } from '../lib/api';

export default function ChatWidget() {
  const { user } = useAuth();
  const [isOpen, setIsOpen] = useState(false);
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState('');
  const [isSending, setIsSending] = useState(false);
  const messagesEndRef = useRef(null);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, isOpen]);

  if (!user) return null;

  const handleSend = async (e) => {
    e.preventDefault();
    const trimmed = input.trim();
    if (!trimmed || isSending) return;

    const userMessage = { role: 'user', text: trimmed };
    const newMessages = [...messages, userMessage];
    setMessages(newMessages);
    setInput('');
    setIsSending(true);

    try {
      const result = await apiFetch(user, '/api/Chat/send', {
        method: 'POST',
        body: JSON.stringify({
          message: trimmed,
          history: newMessages.map((m) => ({ role: m.role, text: m.text })),
        }),
      });

      setMessages((prev) => [...prev, { role: 'model', text: result.reply }]);
    } catch (err) {
      setMessages((prev) => [
        ...prev,
        { role: 'model', text: 'Sorry, something went wrong. Please try again.' },
      ]);
    } finally {
      setIsSending(false);
    }
  };

  return (
    <div className="chat-widget">
      {isOpen && (
        <div className="chat-panel">
          <div className="chat-header">
            <span>CarePulse Assistant</span>
            <button className="chat-close" onClick={() => setIsOpen(false)}>✕</button>
          </div>

          <div className="chat-messages">
            {messages.length === 0 && (
              <div className="chat-empty">
                👋 Hi! Ask me anything about CarePulse or general health questions.
              </div>
            )}
            {messages.map((msg, i) => (
              <div key={i} className={`chat-bubble chat-bubble-${msg.role}`}>
                {msg.text}
              </div>
            ))}
            {isSending && (
              <div className="chat-bubble chat-bubble-model chat-typing">Typing…</div>
            )}
            <div ref={messagesEndRef} />
          </div>

          <form className="chat-input-row" onSubmit={handleSend}>
            <input
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Type a message…"
              disabled={isSending}
            />
            <button type="submit" disabled={isSending || !input.trim()}>Send</button>
          </form>
        </div>
      )}

      <button className="chat-fab" onClick={() => setIsOpen((v) => !v)}>
        {isOpen ? '✕' : '💬'}
      </button>
    </div>
  );
}