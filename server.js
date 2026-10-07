import express from 'express';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const app = express();
const root = path.dirname(fileURLToPath(import.meta.url));
app.use(express.json({ limit: '2mb' }));
app.use(express.static(path.join(root, 'public')));

// Dữ liệu mẫu để trình diễn luồng đăng nhập/cấp tài khoản. Khi triển khai thật,
// thay phần này bằng cơ sở dữ liệu và mật khẩu đã băm.
const users = [
  { id: 1, name: 'Quản trị hệ thống', email: 'admin@ccd.local', password: 'Admin@123', role: 'ADMIN', faculty: 'Trung tâm Học liệu số' },
  { id: 2, name: 'Nguyễn Thu Hương', email: 'ccd@ccd.local', password: 'Ccd@123', role: 'CCD', faculty: 'Trung tâm Học liệu số' },
  { id: 3, name: 'Trần Minh An', email: 'gv@ccd.local', password: 'Giangvien@123', role: 'LECTURER', faculty: 'Khoa Công nghệ thông tin' }
];
const safeUser = ({ password, ...user }) => user;
const sessions = new Map();
function requireAdmin(req, res, next) {
  const token = req.headers.authorization?.replace('Bearer ', '');
  if (!token || sessions.get(token)?.role !== 'ADMIN') return res.status(403).json({ message: 'Chỉ Admin được phép quản lý tài khoản.' });
  next();
}

app.post('/api/login', (req, res) => {
  const { email, password } = req.body || {};
  const user = users.find(u => u.email.toLowerCase() === String(email).toLowerCase() && u.password === password);
  if (!user) return res.status(401).json({ message: 'Email hoặc mật khẩu chưa đúng.' });
  const token = crypto.randomUUID();
  sessions.set(token, safeUser(user));
  res.json({ user: safeUser(user), token });
});
app.get('/api/users', requireAdmin, (_, res) => res.json(users.map(safeUser)));
app.post('/api/users', requireAdmin, (req, res) => {
  const { name, email, role, faculty } = req.body || {};
  if (!name || !email || !['CCD', 'LECTURER'].includes(role)) return res.status(400).json({ message: 'Thông tin tài khoản không hợp lệ.' });
  if (users.some(u => u.email.toLowerCase() === email.toLowerCase())) return res.status(409).json({ message: 'Email đã tồn tại.' });
  const user = { id: Date.now(), name, email, role, faculty: faculty || '', password: 'Welcome@123' };
  users.push(user);
  res.status(201).json({ user: safeUser(user), temporaryPassword: user.password });
});

const rubric = [
  ['Mục tiêu & chuẩn đầu ra', 'Mục tiêu học tập rõ ràng, đo lường được và bám sát chuẩn đầu ra.'],
  ['Nội dung chuyên môn', 'Nội dung đúng, có cấu trúc, cập nhật và có nguồn tham khảo.'],
  ['Hoạt động & đánh giá', 'Có hoạt động học tập và câu hỏi/tiêu chí đánh giá phù hợp.'],
  ['Kỹ thuật & khả năng truy cập', 'Tệp hoạt động tốt, dễ đọc, có mô tả cho hình ảnh và liên kết.']
];

function mockAssessment(data) {
  const text = `${data.title || ''} ${data.clo || ''} ${data.description || ''} ${data.fileName || ''}`.toLowerCase();
  const hasObjective = /mục tiêu|chuẩn đầu ra|outcome|clo/.test(text);
  const hasAssessment = /câu hỏi|đánh giá|quiz|bài tập/.test(text);
  const hasReference = /tham khảo|reference|nguồn/.test(text);
  const technical = /pdf|ppt|scorm|mp4/.test(text);
  const values = [hasObjective ? 23 : 16, hasReference ? 24 : 18, hasAssessment ? 23 : 15, technical ? 21 : 17];
  const criteria = rubric.map(([name, detail], i) => ({ name, detail, score: values[i], max: 25, status: values[i] >= 21 ? 'Đạt' : 'Cần bổ sung' }));
  const total = values.reduce((a,b) => a+b, 0);
  const issues = [];
  if (!hasObjective) issues.push('Bổ sung mục tiêu học tập hoặc chuẩn đầu ra có thể đo lường.');
  if (!hasReference) issues.push('Thêm tài liệu tham khảo/nguồn nội dung để phục vụ thẩm định.');
  if (!hasAssessment) issues.push('Bổ sung câu hỏi, bài tập hoặc tiêu chí đánh giá người học.');
  if (!technical) issues.push('Nên nộp định dạng PDF, PPTX, SCORM hoặc MP4 và kiểm tra liên kết.');
  return { total, verdict: total >= 80 ? 'Đạt – chờ CCD nghiệm thu' : 'Cần chỉnh sửa', criteria, issues, summary: total >= 80 ? 'Học liệu có cấu trúc cơ bản tốt. CCD cần kiểm tra chuyên môn lần cuối.' : 'AI đã phát hiện một số tiêu chí chưa thể hiện rõ trong hồ sơ nộp.' };
}

async function openAIAssessment(data) {
  const prompt = `Bạn là chuyên gia kiểm định học liệu số đại học. Hãy đánh giá hồ sơ học liệu sau theo thang 100: ${JSON.stringify(data)}. Trả về DUY NHẤT JSON hợp lệ với cấu trúc {total:number, verdict:string, summary:string, criteria:[{name:string,detail:string,score:number,max:25,status:string}], issues:string[]}. Bốn tiêu chí là Mục tiêu & chuẩn đầu ra, Nội dung chuyên môn, Hoạt động & đánh giá, Kỹ thuật & khả năng truy cập. Viết tiếng Việt, đưa khuyến nghị cụ thể, không bịa nội dung không được cung cấp.`;
  const response = await fetch('https://api.openai.com/v1/responses', {
    method: 'POST', headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${process.env.OPENAI_API_KEY}` },
    body: JSON.stringify({ model: process.env.OPENAI_MODEL || 'gpt-4.1-mini', input: prompt, text: { format: { type: 'json_object' } } })
  });
  if (!response.ok) throw new Error(`OpenAI API ${response.status}`);
  const json = await response.json();
  return JSON.parse(json.output_text);
}

app.post('/api/assessments', async (req, res) => {
  const data = req.body || {};
  try {
    const assessment = process.env.OPENAI_API_KEY ? await openAIAssessment(data) : mockAssessment(data);
    res.json({ ...assessment, mode: process.env.OPENAI_API_KEY ? 'AI' : 'demo', assessedAt: new Date().toISOString() });
  } catch (error) {
    res.status(502).json({ message: 'Không thể gọi AI. Vui lòng thử lại hoặc dùng chế độ demo.', detail: error.message });
  }
});
app.get('*', (_, res) => res.sendFile(path.join(root, 'public', 'index.html')));
app.listen(process.env.PORT || 3000, () => console.log('Digital Learning QA: http://localhost:3000'));
