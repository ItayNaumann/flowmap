# Queue & Microservice Architecture

## Queues בין שירותים

### ציינו 2 יתרונות בשימוש בתורים בין שירותים במקום HTTP

מאפשר לבחור כמה בקשות מטופלות פרק זמן מסויים לפי קצב הפריקה של ההודעות ולא להצטרך לעשות retries מהservice ששלח את הבקשה אחרי שעוברים את הכמות בקשות האפשריות בפרק זמן שהוגדר

מאפשר שמירה של מידע גם במקרה ונופל פוד שעובד על הבקשות, ניתן לא לעשות commit עד שמסיימים לעבוד ובמקרה וזה נפל התור יכול לתת את המשימה לפוד אחד שיעבוד על זה

---

## הודעות גדולות

### האם מומלץ להעביר הודעה בגודל 100MB? האם יש דרך טובה יותר?

אפשר להעלות את המידע למקום ששומר אובייקטים כמו S3 ולהעביר מידע קטן בין הservices שמודיע איפה שמרתי את המידע ושמתי שצריך את המידע הזה יקראו אותו ישירות מהS3 (או כל דבר מקביל) ושם ממומש דרך יעילה ומקבילית שקוראת את המידע במהירות ומעבירים כמה שפחות מידע בבקשות רשת / תורים

---

## Throughput Calculation

```
טל היא משתמשת מתעללת – כמויות מטורפות של הרצות על כלל השירותים.  לפניכם קובץ אקסל abusive_tal.xlsx עם 2 גרפים:
גרף שמתאר את כמות ההודעות שנשלח על ידי טל בכל שעה.
גרף שמתאר את גודל ההודעות הממוצע שנשלח על ידי טל בכל חצי שעה.
עליכם לחשב מה הthroughput (B/s או בתים לשנייה) וגודל הודעה מקסימלית שהתור אמור לדעת להכיל לכלל המשתמשים במערכת (אם זה היה פלואו). שימו לב כי יש מספר תשובות לזה ועליכם להציג את השיקולים ולהגיע לתשובה סופית. זכרו – טל בן אדם מיוחד, א בל יש משתמשים שעושים דברים דומים.
```

אז אנחנו לא יודע כל כך את כל המידע, אבל אנחנו אנשים של מחקר, לכן ניצור לנו פונקציה יפייפיה שתחשב לנו את כמות הthroughput שנצטרך בתור שלנו כמו אנשים מתורבתים

```python
def required_throughput(
    num_legit_users,
    avg_rate_per_user, # legit per-user rate (bytes/sec)
    abuser_rates=None, # list of measured rates for known abusive users
    user_rate_limit=None, # per-user rate-limit ceiling you plan to enforce
    concurrency_factor=1.0, # fraction of users realistically active at once (0-1)
    safety_factor=9  # headroom for bursts
):
    """
    Scales from a single-user model to a full user base.

    num_legit_users:     total legitimate users
    avg_rate_per_user:    average sustained rate per legit user
    abuser_rates:         known abusers' measured rates (list, can be empty/None)
    user_rate_limit:           if set, clamps EACH abuser's contribution to this value — models capacity needed AFTER rate-limiting is enforced
    concurrency_factor:    e.g. 0.1 means only 10% of users are active in a given window — matters a lot at thousands-of-users scale, since you almost never see 100% simultaneous activity
    """
    abuser_rates = abuser_rates or []

    total_legit = num_legit_users * avg_rate_per_user * concurrency_factor

    total_abuser = sum(
        min(r, user_rate_limit) if user_rate_limit is not None else r
        for r in abuser_rates
    )

    return (total_legit + total_abuser) * safety_factor


# Calculation for peak hours, assuming 10000 legit users
# From tal graph we can see that each message is about ~2.0KB so we assume its about the baseline for a normal message
# Assuming that each user sends 20 messages per minute, so on average each user sends 0.67KB/sec ≈ (2KB * 20 / 60)
# abuser_byte_rate = 948 * 1024 / 1800 ≈ 540 bytes/sec (peak)

print(required_throughput(
    num_legit_users=10000,
    avg_rate_per_user=0.67,
    concurrency_factor=0.90,
    abuser_rates=[ 540.0 for _ in range(25) ] , # calculated for tal, lets assume there are 25 tals in the system, so 25 * 540 = 2700 bytes/sec
    user_rate_limit=None, # no rate limit enforced yet
    safety_factor=9
)) # 175770 bytes / sec = 172 KB/sec
# so 172 KB/sec is a cool estimate for the throughput
```

---

## Delivery Guarantees

### At-most-once

מידע נשלח פעם אחת בלבד, ומקווים שהוא יגיע, לא נשלח מחדש ופשוט מאבדים אותו, הproducer שולח את ההודעה ולא מחכה לאישור ולא מנסה לשלוח מחדש במקרה ולא עובד

זה הכי קל לממש, אין מידע כפול אף פעם ועם הכי פחות Latency וoverhead.

מידע לא בהכרח יגיע ולרוב לא נדע שבכלל היה אמור להישלח, לא מתאים כשכל הודעה חשובה

### At-least-once

מידע תמיד יגיע לשרת, אבל יכול להיות שיגיע יותר מפעם אחת, הproducer ינסה לשלוח שוב עד שהוא יקבל אישור שהבקשה התקבלה, אם האישור נאבד הproducer ישלח אותו שוב.

מונע איבודי מידע, קל למימוש

יכול לגרום למידע כפול ופעולות שחוזרות על עצמן, חייב שהניהול אחרי קבלת המידע ידע להתמודד עם מידע זהה שנשלח פעמיים

### Exactly-once

עובדים על הודעה פעם אחת בידיוק, אין איבוד מידע ואין שכפול מידע. משתמש בשילוב של להבין שמידע כבר נשלח ע"י ID של הודעה ושל שולח ושימוש בטרנזקציות

לא מאבדים שום מידע ולא מעבדים שום מידע פעמים, טוב לתרנזקציות All around כמו העברת כסף או שינוי inventory

עם הכי הרבה latency וoverhead, צריך עדיין לממש את החלק של התרנזקציה בconsumer

---

## Window Processing

### ציינו 2 יתרונות



---

## Watermarks

### מה זה Watermark?

<!-- Answer -->

### יתרון וחסרון של LWM

<!-- Answer -->

### יתרון וחסרון של HWM

<!-- Answer -->

---

## CQRS

### מהו CQRS ומתי נשתמש בו?

<!-- Answer -->

---

## Ambassador Pattern

### מה הוא בא לפתור?

<!-- Answer -->

---

## Sidecar Pattern

### מה המטרה שלו?

<!-- Answer -->

---

## Dead Letter Queue (DLQ)

### הסבירו מה זה DLQ

<!-- Answer -->

---

## Change Data Capture (CDC)

### הסבירו מה זה CDC

<!-- Answer -->

---

## Idempotency

### מה זה ולמה זה חשוב?

<!-- Answer -->

---

## Thundering Herd Problem

### מה זה? איפה אתם מכירים שזה קיים?

<!-- Answer -->

---

## Jittering & Staggering

### מה זה Jittering?

<!-- Answer -->

### מה זה Staggering?

<!-- Answer -->

### מה ההבדל ביניהם?

<!-- Answer -->

### כיצד הם פותרים את Thundering Herd Problem?

<!-- Answer -->

---

## Queue-Based Load Leveling

### איך זה קשור ל-Staggering ול-Jittering?

<!-- Answer -->

### איפה אפשר להשתמש בזה?

<!-- Answer -->

---

## תכנון מערכת מבוססת תור

### תכננו מנגנון מבוסס תור העונה על הדרישות

#### הודעות כושלות לא יעכבו הודעות חדשות

<!-- Answer -->

#### המערכת תימנע ככל האפשר מעיבוד כפול

<!-- Answer -->

#### כל שירות יעבד בדיוק את כמות הבקשות שהוא מסוגל להתמודד איתה

<!-- Answer -->

#### המערכת תוכל לגדול אוטומטית במקרה של עומס

<!-- Answer -->

#### כיצד מטפלים בהודעות כושלות?

<!-- Answer -->

#### מתי מבצעים ACK?

<!-- Answer -->

#### כיצד מונעים השפעה של עיבוד כפול?

<!-- Answer -->

#### כיצד שולטים בקצב הקריאה?

<!-- Answer -->

#### כיצד מבצעים Scaling לשירות?

<!-- Answer --># מבוא לדאטא - חלק 2: Queue & Microservice Architecture

## Queues בין שירותים

### ציינו 2 יתרונות בשימוש בתורים בין שירותים במקום HTTP

<!-- Answer -->

---

## הודעות גדולות

### האם מומלץ להעביר הודעה בגודל 100MB? האם יש דרך טובה יותר?

<!-- Answer -->

---

## Throughput Calculation

### חשבו Throughput וגודל הודעה מקסימלי על בסיס הקובץ abusive_tal.xlsx

<!-- Answer -->

---

## Delivery Guarantees

### At-most-once

<!-- Answer -->

### At-least-once

<!-- Answer -->

### Exactly-once

<!-- Answer -->

---

## Window Processing

### ציינו 2 יתרונות

<!-- Answer -->

---

## Watermarks

### מה זה Watermark?

<!-- Answer -->

### יתרון וחסרון של LWM

<!-- Answer -->

### יתרון וחסרון של HWM

<!-- Answer -->

---

## CQRS

### מהו CQRS ומתי נשתמש בו?

<!-- Answer -->

---

## Ambassador Pattern

### מה הוא בא לפתור?

<!-- Answer -->

---

## Sidecar Pattern

### מה המטרה שלו?

<!-- Answer -->

---

## Dead Letter Queue (DLQ)

### הסבירו מה זה DLQ

<!-- Answer -->

---

## Change Data Capture (CDC)

### הסבירו מה זה CDC

<!-- Answer -->

---

## Idempotency

### מה זה ולמה זה חשוב?

<!-- Answer -->

---

## Thundering Herd Problem

### מה זה? איפה אתם מכירים שזה קיים?

<!-- Answer -->

---

## Jittering & Staggering

### מה זה Jittering?

<!-- Answer -->

### מה זה Staggering?

<!-- Answer -->

### מה ההבדל ביניהם?

<!-- Answer -->

### כיצד הם פותרים את Thundering Herd Problem?

<!-- Answer -->

---

## Queue-Based Load Leveling

### איך זה קשור ל-Staggering ול-Jittering?

<!-- Answer -->

### איפה אפשר להשתמש בזה?

<!-- Answer -->

---

## תכנון מערכת מבוססת תור

### תכננו מנגנון מבוסס תור העונה על הדרישות

#### הודעות כושלות לא יעכבו הודעות חדשות

<!-- Answer -->

#### המערכת תימנע ככל האפשר מעיבוד כפול

<!-- Answer -->

#### כל שירות יעבד בדיוק את כמות הבקשות שהוא מסוגל להתמודד איתה

<!-- Answer -->

#### המערכת תוכל לגדול אוטומטית במקרה של עומס

<!-- Answer -->

#### כיצד מטפלים בהודעות כושלות?

<!-- Answer -->

#### מתי מבצעים ACK?

<!-- Answer -->

#### כיצד מונעים השפעה של עיבוד כפול?

<!-- Answer -->

#### כיצד שולטים בקצב הקריאה?

<!-- Answer -->

#### כיצד מבצעים Scaling לשירות?

<!-- Answer -->
