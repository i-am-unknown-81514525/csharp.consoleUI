```
-> AddBuffer()
-> ValidateExternal(): LockStatus
-> GetLock()?
-> Reset()
-> HandleExternal()
<- LockChangeAnnounce()
-> Reset() (depend on last)
-> DropUnused()
```
`LockChangeAnnounce` can only be use to downgrade the lock.
