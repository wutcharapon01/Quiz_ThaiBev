export const questionBank = [
  {
    id: 1,
    title: 'Question 1 (IT-01)',
    description: 'Add/View modal with SQLite API',
    prompt: 'This question uses a custom screen from IT-01 requirement.',
    choices: ['Open screen from dashboard']
  },
  {
    id: 2,
    title: 'Question 2 (IT-02)',
    description: 'Login + Register + JWT Validation',
    prompt: 'This question uses a custom login system with JWT authentication.',
    choices: ['Open login screen']
  },
  {
    id: 3,
    title: 'Question 3 (IT-03)',
    description: 'อนุมัติ/ไม่อนุมัติรายการ พร้อมเหตุผล',
    prompt: 'หน้านี้เป็นระบบเอกสาร mockup พร้อม modal อนุมัติและไม่อนุมัติ',
    choices: ['Open approval screen']
  },
  {
    id: 4,
    title: 'Question 4 (IT-04)',
    description: 'ฟอร์มบันทึกข้อมูลพร้อม validation',
    prompt: 'ตรวจสอบ Email/Phone/Birth Day และบันทึกรูปแบบ Base64',
    choices: ['Open profile form']
  },
  {
    id: 5,
    title: 'Question 5 (IT-05)',
    description: 'ระบบรับบัตรคิวและล้างคิว',
    prompt: 'สร้างคิวแบบ Running จาก A0 ถึง Z9 และล้างกลับ 00',
    choices: ['Open queue screen']
  },
  {
    id: 6,
    title: 'Question 6 (IT-06)',
    description: 'เพิ่ม/ลบรหัสสินค้าและแสดง Barcode Code39',
    prompt: 'รองรับรูปแบบ XXXX-XXXX-XXXX-XXXX และยืนยันก่อนลบ',
    choices: ['Open barcode screen']
  },
  {
    id: 7,
    title: 'Question 7 (IT-07)',
    description: 'เพิ่ม/ลบรหัสสินค้าและดู QR Code',
    prompt: 'รูปแบบ XXXXX-XXXXX-XXXXX-XXXXX-XXXXX-XXXXX และห้ามซ้ำ',
    choices: ['Open QR screen']
  },
  {
    id: 8,
    title: 'Question 8 (IT-08)',
    description: 'เพิ่ม/ลบข้อสอบพร้อมจัด Running Number',
    prompt: 'เพิ่มผ่านฟอร์ม IT08-2 และลบพร้อมจัดลำดับใหม่อัตโนมัติ',
    choices: ['Open exam manager']
  },
  {
    id: 9,
    title: 'Question 9 (IT-09)',
    description: 'แสดงโพสต์และคอมเมนต์ด้วย Enter',
    prompt: 'ผู้คอมเมนต์เป็น Blend 285 และกด Enter เพื่อเพิ่มข้อความ',
    choices: ['Open comment screen']
  },
  {
    id: 10,
    title: 'Question 10 (IT-10)',
    description: 'ระบบสอบแบบเลือกตอบ 1 คำตอบต่อข้อ พร้อมบันทึกผล',
    prompt: 'กรอกชื่อ เลือกคำตอบ ส่งข้อสอบ บันทึกผล และสอบใหม่ได้',
    choices: ['Open IT10 exam screen']
  }
]

export function getQuestionById(id) {
  const numericId = Number(id)
  return questionBank.find((question) => question.id === numericId) ?? null
}

